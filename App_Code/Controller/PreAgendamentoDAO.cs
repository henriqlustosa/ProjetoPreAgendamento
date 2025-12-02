using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

public class PreAgendamentoDAO
{
    private static readonly string connStr = ConfigurationManager.ConnectionStrings["gtaConnectionString"].ConnectionString;
    public static void Aprovar(int id, string usuarioLogado)
    {
        using (SqlConnection con = new SqlConnection(connStr))
        {
            // Atualiza status para 'V' (Verificado/Aprovado)
            // Também registra quem aprovou e quando
            string sql = @"
            UPDATE dbo.PreAgendamento 
            SET status = 'V', 
                usuario_atualizacao = @user, 
                data_atualizacao = GETDATE() 
            WHERE id = @id";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", id);

                if (string.IsNullOrEmpty(usuarioLogado))
                    cmd.Parameters.AddWithValue("@user", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@user", usuarioLogado);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
    public static void Excluir(int id, string usuarioLogado)
    {
        using (SqlConnection con = new SqlConnection(connStr))
        {
            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                // =================================================================
                // 1. INATIVAR TABELAS FILHAS
                // =================================================================

                // 1.1 Dias
                string sqlInatDias = @"
                UPDATE dbo.PreAgendamentoDia 
                SET status = 'I', data_exclusao = GETDATE() 
                WHERE id_preagendamento = @id AND status = 'A'";

                using (SqlCommand cmd = new SqlCommand(sqlInatDias, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                // 1.2 Bloqueios
                string sqlInatBloq = @"
                UPDATE dbo.PreAgendamentoBloqueio 
                SET status = 'I', data_exclusao = GETDATE() 
                WHERE id_preagendamento = @id AND status = 'A'";

                using (SqlCommand cmd = new SqlCommand(sqlInatBloq, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                // 1.3 Períodos (Atenção ao Case Sensitivity das colunas conforme seu método Atualizar)
                string sqlInatPer = @"
                UPDATE dbo.PreAgendamentoPeriodo 
                SET Status = 'I', DataExclusao = GETDATE()
                WHERE IdPreAgendamento = @id AND Status = 'A'";

                using (SqlCommand cmd = new SqlCommand(sqlInatPer, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                // =================================================================
                // 2. INATIVAR TABELA PAI (PreAgendamento)
                // =================================================================
                string sqlPai = @"UPDATE dbo.PreAgendamento 
                              SET data_exclusao = GETDATE(), 
                                  usuario_excluir = @usuario,
                                  status = 'I'
                              WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(sqlPai, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    if (string.IsNullOrEmpty(usuarioLogado))
                        cmd.Parameters.AddWithValue("@usuario", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@usuario", usuarioLogado);

                    cmd.ExecuteNonQuery();
                }

                // Se chegou até aqui sem erro, confirma tudo no banco
                tran.Commit();
            }
            catch (Exception ex)
            {
                // Se der erro em qualquer etapa, desfaz tudo
                tran.Rollback();
                throw new Exception("Erro ao excluir registro e dependências: " + ex.Message);
            }
        }
    }
    // =================================================================================
    // INSERT (Novo Registro)
    // =================================================================================
    public static int Inserir(PreAgendamentoDTO dto, List<BlocoDiaDTO> blocos, List<BloqueioDTO> bloqueios)
    {
        int novoId = 0;

        using (SqlConnection con = new SqlConnection(connStr))
        {
            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                // 1. PAI (PreAgendamento)
                string sqlPai = @"
                    INSERT INTO dbo.PreAgendamento
                        (data_preenchimento, observacoes, usuario, data_cadastro, 
                         cod_especialidade, cod_profissional, status)
                    VALUES
                        (@dt, @obs, @user, GETDATE(), 
                         @esp, @prof, 'A');
                    SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(sqlPai, con, tran))
                {
                    cmd.Parameters.AddWithValue("@dt", dto.DataPreenchimento);
                    cmd.Parameters.AddWithValue("@obs", dto.Observacoes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@user", dto.Usuario);
                    cmd.Parameters.AddWithValue("@esp", dto.CodEspecialidade);
                    cmd.Parameters.AddWithValue("@prof", dto.CodProfissional);

                    novoId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 2. DIAS (PreAgendamentoDia)
                if (blocos != null && blocos.Count > 0)
                {
                    string sqlDia = @"
                        INSERT INTO dbo.PreAgendamentoDia
                        (id_preagendamento, dia_semana, horario, consultas_novas, consultas_retorno, 
                         cod_subespecialidade, status, data_cadastro)
                        VALUES
                        (@id, @dia, @hor, @nov, @ret, @sub, 'A', GETDATE())";

                    foreach (var b in blocos)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlDia, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", novoId);
                            cmd.Parameters.AddWithValue("@dia", b.DiaSemana);
                            cmd.Parameters.AddWithValue("@hor", b.Horario ?? (object)DBNull.Value);

                            int qNov = 0, qRet = 0;
                            int.TryParse(b.ConsultasNovas, out qNov);
                            int.TryParse(b.ConsultasRetorno, out qRet);
                            cmd.Parameters.AddWithValue("@nov", qNov);
                            cmd.Parameters.AddWithValue("@ret", qRet);
                            // Declara a variável para receber o valor convertido
                            int idSub;

                            // Tenta converter a string para inteiro. Se der certo E for maior que 0, usa o valor.
                            if (int.TryParse(b.CodSubespecialidade, out idSub) && idSub > 0)
                            {
                                cmd.Parameters.AddWithValue("@sub", idSub);
                            }
                            else
                            {
                                // Caso contrário (se for vazio, nulo ou "0"), grava NULL no banco
                                cmd.Parameters.AddWithValue("@sub", DBNull.Value);
                            }

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // 3. BLOQUEIOS (PreAgendamentoBloqueio) - ATUALIZADO PARA cod_motivo
                if (bloqueios != null && bloqueios.Count > 0)
                {
                    string sqlBloq = @"
                        INSERT INTO dbo.PreAgendamentoBloqueio
                        (id_preagendamento, data_de, data_ate, cod_motivo, status, data_cadastro)
                        VALUES
                        (@id, @de, @ate, @cod_motivo, 'A', GETDATE())";

                    foreach (var bl in bloqueios)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlBloq, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", novoId);

                            DateTime dtDe, dtAte;
                            if (!DateTime.TryParse(bl.De, out dtDe)) dtDe = DateTime.Now;
                            if (!DateTime.TryParse(bl.Ate, out dtAte)) dtAte = DateTime.Now;

                            cmd.Parameters.AddWithValue("@de", dtDe);
                            cmd.Parameters.AddWithValue("@ate", dtAte);

                            // Grava o ID do motivo
                            if (bl.CodMotivo > 0)
                                cmd.Parameters.AddWithValue("@cod_motivo", bl.CodMotivo);
                            else
                                cmd.Parameters.AddWithValue("@cod_motivo", DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        return novoId;
    }

    // =================================================================================
    // UPDATE (Atualização com Soft Delete e Rastreio de Usuário)
    // =================================================================================
    public static void Atualizar(PreAgendamentoDTO dto, List<BlocoDiaDTO> blocos, List<BloqueioDTO> bloqueios, List<PeriodoPreAgendamentoDTO> periodos, string usuarioLogado)
    {
        using (SqlConnection con = new SqlConnection(connStr))
        {
            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                // 1. ATUALIZA PAI (PreAgendamento)
                // Adicionado o campo usuario_atualizacao
                string sqlUpdatePai = @"
                UPDATE dbo.PreAgendamento
                SET data_preenchimento = @dt,
                    cod_especialidade = @esp,
                    cod_profissional = @prof,
                    observacoes = @obs,
                    usuario = @user,
                    usuario_atualizacao = @usuario_atualizacao, 
                    data_atualizacao = GETDATE()
                WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(sqlUpdatePai, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", dto.Id);
                    cmd.Parameters.AddWithValue("@dt", dto.DataPreenchimento);
                    cmd.Parameters.AddWithValue("@esp", dto.CodEspecialidade);
                    cmd.Parameters.AddWithValue("@prof", dto.CodProfissional);
                    cmd.Parameters.AddWithValue("@obs", dto.Observacoes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@user", dto.Usuario); // Usuário 'dono' do registro (se aplicável)

                    // Tratamento do Usuário que está fazendo a ATUALIZAÇÃO agora
                    if (string.IsNullOrEmpty(usuarioLogado))
                        cmd.Parameters.AddWithValue("@usuario_atualizacao", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@usuario_atualizacao", usuarioLogado);

                    cmd.ExecuteNonQuery();
                }

                // 2. DIAS
                string sqlInatDias = @"
                UPDATE dbo.PreAgendamentoDia 
                SET status = 'I', data_exclusao = GETDATE() 
                WHERE id_preagendamento = @id AND status = 'A'";

                using (SqlCommand cmd = new SqlCommand(sqlInatDias, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", dto.Id);
                    cmd.ExecuteNonQuery();
                }

                if (blocos != null && blocos.Count > 0)
                {
                    string sqlInsDia = @"
                    INSERT INTO dbo.PreAgendamentoDia
                    (id_preagendamento, dia_semana, horario, consultas_novas, consultas_retorno, 
                     cod_subespecialidade, status, data_cadastro)
                    VALUES (@id, @dia, @hor, @nov, @ret, @sub, 'A', GETDATE())";

                    foreach (var b in blocos)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlInsDia, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", dto.Id);
                            cmd.Parameters.AddWithValue("@dia", b.DiaSemana);
                            cmd.Parameters.AddWithValue("@hor", b.Horario ?? (object)DBNull.Value);

                            int qNov = 0, qRet = 0;
                            int.TryParse(b.ConsultasNovas, out qNov);
                            int.TryParse(b.ConsultasRetorno, out qRet);
                            cmd.Parameters.AddWithValue("@nov", qNov);
                            cmd.Parameters.AddWithValue("@ret", qRet);

                            // Declara a variável para receber o valor convertido
                            int idSub;

                            // Tenta converter a string para inteiro. Se der certo E for maior que 0, usa o valor.
                            if (int.TryParse(b.CodSubespecialidade, out idSub) && idSub > 0)
                            {
                                cmd.Parameters.AddWithValue("@sub", idSub);
                            }
                            else
                            {
                                // Caso contrário (se for vazio, nulo ou "0"), grava NULL no banco
                                cmd.Parameters.AddWithValue("@sub", DBNull.Value);
                            }

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // 3. BLOQUEIOS
                string sqlInatBloq = @"
                UPDATE dbo.PreAgendamentoBloqueio 
                SET status = 'I', data_exclusao = GETDATE() 
                WHERE id_preagendamento = @id AND status = 'A'";

                using (SqlCommand cmd = new SqlCommand(sqlInatBloq, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", dto.Id);
                    cmd.ExecuteNonQuery();
                }

                if (bloqueios != null && bloqueios.Count > 0)
                {
                    string sqlInsBloq = @"
                    INSERT INTO dbo.PreAgendamentoBloqueio
                    (id_preagendamento, data_de, data_ate, cod_motivo, status, data_cadastro)
                    VALUES (@id, @de, @ate, @cod_motivo, 'A', GETDATE())";

                    foreach (var bl in bloqueios)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlInsBloq, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", dto.Id);

                            DateTime d1, d2;
                            DateTime.TryParse(bl.De, out d1);
                            DateTime.TryParse(bl.Ate, out d2);

                            cmd.Parameters.AddWithValue("@de", d1);
                            cmd.Parameters.AddWithValue("@ate", d2);

                            if (bl.CodMotivo > 0)
                                cmd.Parameters.AddWithValue("@cod_motivo", bl.CodMotivo);
                            else
                                cmd.Parameters.AddWithValue("@cod_motivo", DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // 4. PERÍODOS
                string sqlInatPer = @"
                UPDATE dbo.PreAgendamentoPeriodo 
                SET Status = 'I' , DataExclusao = GETDATE()
                WHERE IdPreAgendamento = @id AND Status = 'A'";

                using (SqlCommand cmd = new SqlCommand(sqlInatPer, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", dto.Id);
                    cmd.ExecuteNonQuery();
                }

                if (periodos != null && periodos.Count > 0)
                {
                    // Nota: Mantivemos @user (dto.Usuario) aqui pois geralmente é o 'dono' do registro. 
                    // Se quiser que seja o usuário que está editando, troque @user por usuarioLogado aqui também.
                    string sqlInsPer = @"
                    INSERT INTO dbo.PreAgendamentoPeriodo
                    (IdPreAgendamento, Ano, Mes, DataInicio, DataFim, Status, DataCadastro, UsuarioCadastro)
                    VALUES (@id, @ano, @mes, @dIni, @dFim, 'A', GETDATE(), @user)";

                    foreach (var p in periodos)
                    {
                        DateTime dIni = new DateTime(p.Ano, p.Mes, 1);
                        DateTime dFim = dIni.AddMonths(1).AddDays(-1);

                        using (SqlCommand cmd = new SqlCommand(sqlInsPer, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", dto.Id);
                            cmd.Parameters.AddWithValue("@ano", p.Ano);
                            cmd.Parameters.AddWithValue("@mes", p.Mes);
                            cmd.Parameters.AddWithValue("@dIni", dIni);
                            cmd.Parameters.AddWithValue("@dFim", dFim);
                            cmd.Parameters.AddWithValue("@user", dto.Usuario);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw new Exception("Erro ao atualizar: " + ex.Message);
            }
        }
    }

    public static List<PreAgendamentoDTO> ListarTodos()
    {
        var lista = new List<PreAgendamentoDTO>();

        using (SqlConnection con = new SqlConnection(connStr))
        {
            string sql = @"
                SELECT  
                    p.id,
                    p.data_preenchimento,
                    p.cod_especialidade,
                    e.nm_especialidade        AS clinica,
                    p.cod_profissional,
                    pr.nome_profissional      AS profissional
                FROM dbo.PreAgendamento p
                LEFT JOIN dbo.Especialidade e
                       ON e.cod_especialidade = p.cod_especialidade
                LEFT JOIN dbo.Profissional pr
                       ON pr.cod_profissional = p.cod_profissional
                WHERE p.status = 'A'
                ORDER BY p.id DESC;";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var dto = new PreAgendamentoDTO
                        {
                            Id = Convert.ToInt32(dr["id"]),
                            DataPreenchimento = Convert.ToDateTime(dr["data_preenchimento"]),
                            Clinica = dr["clinica"] == DBNull.Value ? null : dr["clinica"].ToString(),
                            Profissional = dr["profissional"] == DBNull.Value ? null : dr["profissional"].ToString()
                        };

                        if (dr["cod_especialidade"] != DBNull.Value)
                            dto.CodEspecialidade = Convert.ToInt32(dr["cod_especialidade"]);

                        if (dr["cod_profissional"] != DBNull.Value)
                            dto.CodProfissional = Convert.ToInt32(dr["cod_profissional"]);

                        lista.Add(dto);
                    }
                }
            }
        }
        return lista;
    }

    public static List<BlocoDiaDTO> ListarBlocos(int idPre)
    {
        var lista = new List<BlocoDiaDTO>();
        using (SqlConnection con = new SqlConnection(connStr))
        {
            string sql = @"
                SELECT d.dia_semana, d.horario, d.consultas_novas, d.consultas_retorno, 
                       d.cod_subespecialidade, s.nm_subespecialidade
                FROM dbo.PreAgendamentoDia d
                LEFT JOIN dbo.SubEspecialidade s ON d.cod_subespecialidade = s.cod_subespecialidade
                WHERE d.id_preagendamento = @id 
                  AND d.status = 'A'";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idPre);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var b = new BlocoDiaDTO();
                        b.DiaSemana = dr["dia_semana"].ToString();
                        b.Horario = dr["horario"].ToString();
                        b.ConsultasNovas = dr["consultas_novas"].ToString();
                        b.ConsultasRetorno = dr["consultas_retorno"].ToString();

                        if (dr["cod_subespecialidade"] != DBNull.Value)
                        {
                            b.CodSubespecialidade = dr["cod_subespecialidade"].ToString();
                            b.SubespecialidadeTexto = dr["nm_subespecialidade"].ToString();
                        }
                        lista.Add(b);
                    }
                }
            }
        }
        return lista;
    }

    // =================================================================================
    // LEITURA DE BLOQUEIOS - COM JOIN PARA PREENCHER MotivoTexto
    // =================================================================================
    public static List<BloqueioDTO> ListarBloqueios(int idPre)
    {
        var lista = new List<BloqueioDTO>();
        using (SqlConnection con = new SqlConnection(connStr))
        {
            // Query alterada: Alias 'B' para Bloqueio e 'M' para Motivo
            string sql = @"
            SELECT 
                B.data_de, 
                B.data_ate, 
                B.cod_motivo,
                M.nm_motivo
            FROM dbo.PreAgendamentoBloqueio B
            LEFT JOIN dbo.PreAgendamentoMotivo M ON B.cod_motivo = M.cod_motivo
            WHERE B.id_preagendamento = @id 
              AND B.status = 'A'";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idPre);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var b = new BloqueioDTO();

                        // Datas
                        if (dr["data_de"] != DBNull.Value)
                            b.De = Convert.ToDateTime(dr["data_de"]).ToString("yyyy-MM-dd");

                        if (dr["data_ate"] != DBNull.Value)
                            b.Ate = Convert.ToDateTime(dr["data_ate"]).ToString("yyyy-MM-dd");

                        // ID do Motivo
                        if (dr["cod_motivo"] != DBNull.Value)
                            b.CodMotivo = Convert.ToInt32(dr["cod_motivo"]);

                        // Nome do Motivo (Vindo do Join)
                        if (dr["nm_motivo"] != DBNull.Value)
                            b.MotivoTexto = dr["nm_motivo"].ToString();
                        else
                            b.MotivoTexto = "Motivo não encontrado"; // Fallback caso o join falhe

                        lista.Add(b);
                    }
                }
            }
        }
        return lista;
    }
    public static List<PeriodoPreAgendamentoDTO> ListarPeriodos(int idPre)
    {
        var lista = new List<PeriodoPreAgendamentoDTO>();
        using (SqlConnection con = new SqlConnection(connStr))
        {
            string sql = @"
                SELECT Ano, Mes
                FROM dbo.PreAgendamentoPeriodo
                WHERE IdPreAgendamento = @id 
                  AND Status = 'A'";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idPre);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var p = new PeriodoPreAgendamentoDTO();
                        p.Ano = Convert.ToInt32(dr["Ano"]);
                        p.Mes = Convert.ToInt32(dr["Mes"]);
                        lista.Add(p);
                    }
                }
            }
        }
        return lista;
    }

    public static void InserirPeriodos(int idPre, List<PeriodoPreAgendamentoDTO> periodos)
    {
        if (periodos == null || periodos.Count == 0) return;

        string user = "sistema";
        if (HttpContext.Current != null &&
            HttpContext.Current.Session != null &&
            HttpContext.Current.Session["login"] != null)
        {
            user = HttpContext.Current.Session["login"].ToString();
        }

        using (SqlConnection con = new SqlConnection(connStr))
        {
            con.Open();
            foreach (var p in periodos)
            {
                DateTime dIni = new DateTime(p.Ano, p.Mes, 1);
                DateTime dFim = dIni.AddMonths(1).AddDays(-1);

                string sql = @"
                    INSERT INTO dbo.PreAgendamentoPeriodo
                    (IdPreAgendamento, Ano, Mes, DataInicio, DataFim, Status, DataCadastro, UsuarioCadastro)
                    VALUES (@id, @ano, @mes, @dIni, @dFim, 'A', GETDATE(), @user)";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", idPre);
                    cmd.Parameters.AddWithValue("@ano", p.Ano);
                    cmd.Parameters.AddWithValue("@mes", p.Mes);
                    cmd.Parameters.AddWithValue("@dIni", dIni);
                    cmd.Parameters.AddWithValue("@dFim", dFim);
                    cmd.Parameters.AddWithValue("@user", user);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public static PreAgendamentoDTO ObterPorId(int id)
    {
        PreAgendamentoDTO dto = null;
        using (SqlConnection con = new SqlConnection(connStr))
        {
            // CORREÇÃO: Adicionados JOINs para trazer os nomes (Clínica e Profissional)
            string sql = @"
            SELECT 
                p.id, 
                p.data_preenchimento, 
                p.cod_especialidade, 
                e.nm_especialidade AS nome_especialidade, -- Traz o nome da clínica
                p.cod_profissional, 
                pr.nome_profissional,                     -- Traz o nome do médico
                p.observacoes, 
                p.usuario, 
                p.data_cadastro
            FROM dbo.PreAgendamento p
            LEFT JOIN dbo.Especialidade e ON p.cod_especialidade = e.cod_especialidade
            LEFT JOIN dbo.Profissional pr ON p.cod_profissional = pr.cod_profissional
            WHERE p.id = @id";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        dto = new PreAgendamentoDTO();
                        dto.Id = Convert.ToInt32(dr["id"]);
                        dto.DataPreenchimento = Convert.ToDateTime(dr["data_preenchimento"]);
                        dto.Observacoes = dr["observacoes"] == DBNull.Value ? null : dr["observacoes"].ToString();
                        dto.Usuario = dr["usuario"].ToString();
                        dto.DataCadastro = Convert.ToDateTime(dr["data_cadastro"]);

                        if (dr["cod_especialidade"] != DBNull.Value)
                        {
                            dto.CodEspecialidade = Convert.ToInt32(dr["cod_especialidade"]);
                            dto.Clinica = dr["nome_especialidade"].ToString(); // Preenche o nome da clínica
                        }

                        if (dr["cod_profissional"] != DBNull.Value)
                        {
                            dto.CodProfissional = Convert.ToInt32(dr["cod_profissional"]);
                            dto.Profissional = dr["nome_profissional"].ToString(); // CORREÇÃO: Preenche o nome do médico
                        }
                    }
                }
            }
        }
        return dto;
    }
    public static List<ProfissionalDTO> ListarProfissionaisPorEspecialidade(int codEspecialidade)
    {
        List<ProfissionalDTO> lista = new List<ProfissionalDTO>();
        using (SqlConnection con = new SqlConnection(connStr))
        {
            string sql = @"
                SELECT p.cod_profissional, p.nome_profissional
                FROM dbo.Profissional p
                INNER JOIN dbo.ProfissionalEspecialidade pe ON pe.cod_profissional = p.cod_profissional
                WHERE pe.cod_especialidade = @cod
                ORDER BY p.nome_profissional";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@cod", codEspecialidade);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new ProfissionalDTO
                        {
                            CodProfissional = Convert.ToInt32(dr["cod_profissional"]),
                            NomeProfissional = dr["nome_profissional"].ToString()
                        });
                    }
                }
            }
        }
        return lista;
    }
    // =================================================================================
    // LISTAR POR CLÍNICA (ESPECIALIDADE)
    // =================================================================================
    public static List<PreAgendamentoDTO> ListarPorClinica(int codEspecialidade)
    {
        var lista = new List<PreAgendamentoDTO>();

        using (SqlConnection con = new SqlConnection(connStr))
        {
            string sql = @"
                SELECT  
                    p.id,
                    p.data_preenchimento,
                    p.cod_especialidade,
                    e.nm_especialidade      AS clinica,
                    p.cod_profissional,
                    pr.nome_profissional    AS profissional
                FROM dbo.PreAgendamento p
                LEFT JOIN dbo.Especialidade e
                       ON e.cod_especialidade = p.cod_especialidade
                LEFT JOIN dbo.Profissional pr
                       ON pr.cod_profissional = p.cod_profissional
                WHERE p.status = 'A'
                  AND p.cod_especialidade = @cod
                ORDER BY p.id DESC;";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                // Adiciona o parâmetro para filtrar
                cmd.Parameters.AddWithValue("@cod", codEspecialidade);

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var dto = new PreAgendamentoDTO
                        {
                            Id = Convert.ToInt32(dr["id"]),
                            DataPreenchimento = Convert.ToDateTime(dr["data_preenchimento"]),
                            Clinica = dr["clinica"] == DBNull.Value ? null : dr["clinica"].ToString(),
                            Profissional = dr["profissional"] == DBNull.Value ? null : dr["profissional"].ToString()
                        };

                        if (dr["cod_especialidade"] != DBNull.Value)
                            dto.CodEspecialidade = Convert.ToInt32(dr["cod_especialidade"]);

                        if (dr["cod_profissional"] != DBNull.Value)
                            dto.CodProfissional = Convert.ToInt32(dr["cod_profissional"]);

                        lista.Add(dto);
                    }
                }
            }
        }
        return lista;
    }

    // =================================================================================
    //  NOVO MÉTODO: LISTAR APROVADOS (STATUS 'C')
    // =================================================================================
    public static DataTable ListarAprovadosPorClinica()
    {
        using (SqlConnection con = new SqlConnection(connStr))
        {
            // Query com JOINS para trazer nomes legíveis (Clinica/Profissional)
            // Filtra por status = 'C' e pela especialidade do chefe
            string sql = @"
                SELECT 
                    p.id,
                    p.data_preenchimento,
                    p.observacoes,
                    p.usuario,
                    p.data_cadastro,
                    p.cod_especialidade,
                    e.nm_especialidade AS Clinica,           -- Alias 'Clinica' para o Grid
                    p.cod_profissional,
                    pr.nome_profissional AS Profissional,    -- Alias 'Profissional' para o Grid
                    p.data_atualizacao,
                    p.status,
                    p.usuario_atualizacao
                FROM [hspmPreAgendamento].[dbo].[PreAgendamento] p
                LEFT JOIN [hspmPreAgendamento].[dbo].[Especialidade] e 
                    ON e.cod_especialidade = p.cod_especialidade
                LEFT JOIN [hspmPreAgendamento].[dbo].[Profissional] pr 
                    ON pr.cod_profissional = p.cod_profissional
                WHERE p.status = 'V' 
                
                ORDER BY p.data_preenchimento DESC";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
               

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }
    }
}