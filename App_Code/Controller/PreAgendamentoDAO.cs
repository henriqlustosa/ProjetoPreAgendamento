using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

public class PreAgendamentoDAO
{
    private static readonly string connStr = ConfigurationManager.ConnectionStrings["gtaConnectionString"].ConnectionString;

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
                // 1. PAI (PreAgendamento) - Atenção: colunas e status
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
                            cmd.Parameters.AddWithValue("@dia", b.diaSemana);
                            cmd.Parameters.AddWithValue("@hor", b.horario ?? (object)DBNull.Value);

                            int qNov = 0, qRet = 0;
                            int.TryParse(b.consultasNovas, out qNov);
                            int.TryParse(b.consultasRetorno, out qRet);
                            cmd.Parameters.AddWithValue("@nov", qNov);
                            cmd.Parameters.AddWithValue("@ret", qRet);

                            if (b.CodSubespecialidade.HasValue && b.CodSubespecialidade.Value > 0)
                                cmd.Parameters.AddWithValue("@sub", b.CodSubespecialidade.Value);
                            else
                                cmd.Parameters.AddWithValue("@sub", DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // 3. BLOQUEIOS (PreAgendamentoBloqueio)
                if (bloqueios != null && bloqueios.Count > 0)
                {
                    string sqlBloq = @"
                        INSERT INTO dbo.PreAgendamentoBloqueio
                        (id_preagendamento, data_de, data_ate, motivo, status, data_cadastro)
                        VALUES
                        (@id, @de, @ate, @motivo, 'A', GETDATE())";

                    foreach (var bl in bloqueios)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlBloq, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", novoId);

                            DateTime dtDe, dtAte;
                            // Se falhar parse, assume data atual para não quebrar (ou trate como erro)
                            if (!DateTime.TryParse(bl.de, out dtDe)) dtDe = DateTime.Now;
                            if (!DateTime.TryParse(bl.ate, out dtAte)) dtAte = DateTime.Now;

                            cmd.Parameters.AddWithValue("@de", dtDe);
                            cmd.Parameters.AddWithValue("@ate", dtAte);
                            cmd.Parameters.AddWithValue("@motivo", bl.motivo ?? "");

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
    // UPDATE (Atualização com Soft Delete)
    // =================================================================================
    public static void Atualizar(PreAgendamentoDTO dto, List<BlocoDiaDTO> blocos, List<BloqueioDTO> bloqueios, List<PeriodoPreAgendamentoDTO> periodos)
    {
        using (SqlConnection con = new SqlConnection(connStr))
        {
            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                // 1. ATUALIZA PAI (Dados básicos)
                // Nota: data_atualizacao agora existe na tabela pai segundo sua imagem
                string sqlUpdatePai = @"
                    UPDATE dbo.PreAgendamento
                    SET data_preenchimento = @dt,
                        cod_especialidade = @esp,
                        cod_profissional = @prof,
                        observacoes = @obs,
                        usuario = @user,
                        data_atualizacao = GETDATE()
                    WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(sqlUpdatePai, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", dto.Id);
                    cmd.Parameters.AddWithValue("@dt", dto.DataPreenchimento);
                    cmd.Parameters.AddWithValue("@esp", dto.CodEspecialidade);
                    cmd.Parameters.AddWithValue("@prof", dto.CodProfissional);
                    cmd.Parameters.AddWithValue("@obs", dto.Observacoes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@user", dto.Usuario);
                    cmd.ExecuteNonQuery();
                }

                // ------------------------------------------------------------
                // 2. DIAS (Soft Delete + Insert)
                // ------------------------------------------------------------
                // A. Inativar Antigos
                string sqlInatDias = @"
                    UPDATE dbo.PreAgendamentoDia 
                    SET status = 'I', data_exclusao = GETDATE() 
                    WHERE id_preagendamento = @id AND status = 'A'";

                using (SqlCommand cmd = new SqlCommand(sqlInatDias, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", dto.Id);
                    cmd.ExecuteNonQuery();
                }

                // B. Inserir Novos
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
                            cmd.Parameters.AddWithValue("@dia", b.diaSemana);
                            cmd.Parameters.AddWithValue("@hor", b.horario ?? (object)DBNull.Value);

                            int qNov = 0, qRet = 0;
                            int.TryParse(b.consultasNovas, out qNov);
                            int.TryParse(b.consultasRetorno, out qRet);
                            cmd.Parameters.AddWithValue("@nov", qNov);
                            cmd.Parameters.AddWithValue("@ret", qRet);

                            if (b.CodSubespecialidade.HasValue && b.CodSubespecialidade.Value > 0)
                                cmd.Parameters.AddWithValue("@sub", b.CodSubespecialidade.Value);
                            else
                                cmd.Parameters.AddWithValue("@sub", DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // ------------------------------------------------------------
                // 3. BLOQUEIOS (Soft Delete + Insert)
                // ------------------------------------------------------------
                // A. Inativar
                string sqlInatBloq = @"
                    UPDATE dbo.PreAgendamentoBloqueio 
                    SET status = 'I', data_exclusao = GETDATE() 
                    WHERE id_preagendamento = @id AND status = 'A'";

                using (SqlCommand cmd = new SqlCommand(sqlInatBloq, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", dto.Id);
                    cmd.ExecuteNonQuery();
                }

                // B. Inserir Novos
                if (bloqueios != null && bloqueios.Count > 0)
                {
                    string sqlInsBloq = @"
                        INSERT INTO dbo.PreAgendamentoBloqueio
                        (id_preagendamento, data_de, data_ate, motivo, status, data_cadastro)
                        VALUES (@id, @de, @ate, @motivo, 'A', GETDATE())";

                    foreach (var bl in bloqueios)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlInsBloq, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", dto.Id);

                            DateTime d1, d2;
                            DateTime.TryParse(bl.de, out d1);
                            DateTime.TryParse(bl.ate, out d2);

                            cmd.Parameters.AddWithValue("@de", d1);
                            cmd.Parameters.AddWithValue("@ate", d2);
                            cmd.Parameters.AddWithValue("@motivo", bl.motivo ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // ------------------------------------------------------------
                // 4. PERÍODOS (Soft Delete + Insert)
                // ------------------------------------------------------------
                // A. Inativar
                string sqlInatPer = @"
                    UPDATE dbo.PreAgendamentoPeriodo 
                    SET Status = 'I' -- Nota: Periodo não tem data_exclusao na imagem, se tiver adicione
                    WHERE IdPreAgendamento = @id AND Status = 'A'";

                using (SqlCommand cmd = new SqlCommand(sqlInatPer, con, tran))
                {
                    cmd.Parameters.AddWithValue("@id", dto.Id);
                    cmd.ExecuteNonQuery();
                }

                // B. Inserir Novos (Logica existente de calculo de datas)
                if (periodos != null && periodos.Count > 0)
                {
                    string sqlInsPer = @"
                        INSERT INTO dbo.PreAgendamentoPeriodo
                        (IdPreAgendamento, Ano, Mes, DataInicio, DataFim, Status, DataCadastro, UsuarioCadastro)
                        VALUES (@id, @ano, @mes, @dIni, @dFim, 'A', GETDATE(), @user)";

                    foreach (var p in periodos)
                    {
                        // Calcula Inicio e Fim
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

                            // nomes (vindos dos JOINs, com alias)
                            Clinica = dr["clinica"] == DBNull.Value ? null : dr["clinica"].ToString(),
                            Profissional = dr["profissional"] == DBNull.Value ? null : dr["profissional"].ToString()
                        };

                        // códigos gravados na tabela
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
    // LEITURA (Listar Blocos, Bloqueios e Periodos) - FILTRAR ATIVOS!
    // =================================================================================

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
                  AND d.status = 'A'"; // <--- IMPORTANTE: Filtrar ativos

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idPre);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var b = new BlocoDiaDTO();
                        b.diaSemana = dr["dia_semana"].ToString();
                        b.horario = dr["horario"].ToString();
                        b.consultasNovas = dr["consultas_novas"].ToString();
                        b.consultasRetorno = dr["consultas_retorno"].ToString();

                        if (dr["cod_subespecialidade"] != DBNull.Value)
                        {
                            b.CodSubespecialidade = Convert.ToInt32(dr["cod_subespecialidade"]);
                            b.NomeSubespecialidade = dr["nm_subespecialidade"].ToString();
                        }
                        lista.Add(b);
                    }
                }
            }
        }
        return lista;
    }

    public static List<BloqueioDTO> ListarBloqueios(int idPre)
    {
        var lista = new List<BloqueioDTO>();
        using (SqlConnection con = new SqlConnection(connStr))
        {
            string sql = @"
                SELECT data_de, data_ate, motivo
                FROM dbo.PreAgendamentoBloqueio
                WHERE id_preagendamento = @id 
                  AND status = 'A'"; // <--- IMPORTANTE

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idPre);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var b = new BloqueioDTO();
                        if (dr["data_de"] != DBNull.Value) b.de = Convert.ToDateTime(dr["data_de"]).ToString("yyyy-MM-dd");
                        if (dr["data_ate"] != DBNull.Value) b.ate = Convert.ToDateTime(dr["data_ate"]).ToString("yyyy-MM-dd");
                        b.motivo = dr["motivo"].ToString();
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
                  AND Status = 'A'"; // <--- IMPORTANTE

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

    // =================================================================================
    // INSERIR PERIODOS (Caso seja usado fora do Inserir/Atualizar principal)
    // =================================================================================
    public static void InserirPeriodos(int idPre, List<PeriodoPreAgendamentoDTO> periodos)
    {
        if (periodos == null || periodos.Count == 0) return;

        // Se estiver usando esta função avulsa (num contexto de Insert novo), apenas insere.
        // Se for atualização, cuidado para não duplicar ativos. 
        // Vou assumir que esta função é usada apenas no fluxo de INSERT inicial.

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

    // Métodos auxiliares (ObterPorId, ListarProfissionais, etc) mantêm a lógica,
    // apenas garanta que eles leiam as colunas corretas se mudou nome.
    // ...
    public static PreAgendamentoDTO ObterPorId(int id)
    {
        PreAgendamentoDTO dto = null;
        using (SqlConnection con = new SqlConnection(connStr))
        {
            string sql = @"
                SELECT id, data_preenchimento, cod_especialidade, cod_profissional, observacoes, usuario, data_cadastro
                FROM dbo.PreAgendamento WHERE id = @id"; // Se tiver soft delete no pai, adicione AND status = 'A'

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
                        dto.CodEspecialidade = Convert.ToInt32(dr["cod_especialidade"]);
                        dto.CodProfissional = Convert.ToInt32(dr["cod_profissional"]);
                        dto.Observacoes = dr["observacoes"] == DBNull.Value ? null : dr["observacoes"].ToString();
                        dto.Usuario = dr["usuario"].ToString();
                        dto.DataCadastro = Convert.ToDateTime(dr["data_cadastro"]);

                        // Se precisar buscar nome do profissional/clinica, faça joins ou busque separado
                        // Para edição, precisamos principalmente dos IDs para preencher os combos/hiddens
                    }
                }
            }
        }
        return dto;
    }

    public static List<ProfissionalDTO> ListarProfissionaisPorEspecialidade(int codEspecialidade)
    {
        // Mantém igual ao seu código anterior
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
}