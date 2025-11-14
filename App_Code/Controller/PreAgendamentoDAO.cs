using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;


public class PreAgendamentoDAO
{
    private static readonly string connStr =
        System.Configuration.ConfigurationManager.ConnectionStrings["gtaConnectionString"].ConnectionString;



    public static int Inserir(PreAgendamentoDTO dto, List<BlocoDiaDTO> blocos, List<BloqueioDTO> bloqueios)
    {
        int novoId = 0;

        using (SqlConnection con = new SqlConnection(connStr))
        {
            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                // 1) INSERT na tabela pai (PreAgendamento)
                string sqlPai = @"
INSERT INTO PreAgendamento
    (data_preenchimento, clinica, profissional,
     observacoes,
     usuario, data_cadastro)
VALUES
    (@data_preenchimento, @clinica, @profissional,
     @observacoes,
     @usuario, @data_cadastro);
SELECT SCOPE_IDENTITY();";

                SqlCommand cmdPai = new SqlCommand(sqlPai, con, tran);

                cmdPai.Parameters.AddWithValue("@data_preenchimento", dto.DataPreenchimento);
                cmdPai.Parameters.AddWithValue("@clinica", dto.Clinica);
                cmdPai.Parameters.AddWithValue("@profissional", dto.Profissional);

                



                if (string.IsNullOrEmpty(dto.Observacoes))
                    cmdPai.Parameters.AddWithValue("@observacoes", DBNull.Value);
                else
                    cmdPai.Parameters.AddWithValue("@observacoes", dto.Observacoes);

                cmdPai.Parameters.AddWithValue("@usuario", dto.Usuario);
                cmdPai.Parameters.AddWithValue("@data_cadastro", dto.DataCadastro);

                object result = cmdPai.ExecuteScalar();
                novoId = Convert.ToInt32(result);

                // 2) INSERT na tabela filha de dias (PreAgendamentoDia)
                if (blocos != null && blocos.Count > 0)
                {
                    string sqlDia = @"
INSERT INTO PreAgendamentoDia
    (id_preagendamento, dia_semana, horario, consultas_novas, consultas_retorno, subespecialidade)
VALUES
    (@id_preagendamento, @dia_semana, @horario, @consultas_novas, @consultas_retorno, @subespecialidade);";

                    foreach (BlocoDiaDTO b in blocos)
                    {
                        SqlCommand cmdDia = new SqlCommand(sqlDia, con, tran);

                        cmdDia.Parameters.AddWithValue("@id_preagendamento", novoId);
                        cmdDia.Parameters.AddWithValue("@dia_semana", b.diaSemana);

                        if (string.IsNullOrEmpty(b.horario))
                            cmdDia.Parameters.AddWithValue("@horario", DBNull.Value);
                        else
                            cmdDia.Parameters.AddWithValue("@horario", b.horario);

                        if (string.IsNullOrEmpty(b.consultasNovas))
                            cmdDia.Parameters.AddWithValue("@consultas_novas", DBNull.Value);
                        else
                            cmdDia.Parameters.AddWithValue("@consultas_novas", b.consultasNovas);

                        if (string.IsNullOrEmpty(b.consultasRetorno))
                            cmdDia.Parameters.AddWithValue("@consultas_retorno", DBNull.Value);
                        else
                            cmdDia.Parameters.AddWithValue("@consultas_retorno", b.consultasRetorno);

                        if (string.IsNullOrEmpty(b.subespecialidade))
                            cmdDia.Parameters.AddWithValue("@subespecialidade", DBNull.Value);
                        else
                            cmdDia.Parameters.AddWithValue("@subespecialidade", b.subespecialidade);

                        cmdDia.ExecuteNonQuery();
                    }
                }

                // 3) INSERT na tabela de bloqueios (PreAgendamentoBloqueio)
                if (bloqueios != null && bloqueios.Count > 0)
                {
                    string sqlBloq = @"
INSERT INTO PreAgendamentoBloqueio
    (id_preagendamento, data_de, data_ate, motivo)
VALUES
    (@id_preagendamento, @data_de, @data_ate, @motivo);";

                    foreach (BloqueioDTO bl in bloqueios)
                    {
                        SqlCommand cmdBloq = new SqlCommand(sqlBloq, con, tran);

                        cmdBloq.Parameters.AddWithValue("@id_preagendamento", novoId);

                        // Campos 'de' e 'ate' vêm como string (type='date' => yyyy-MM-dd)
                        DateTime dataDe;
                        DateTime dataAte;

                        if (!string.IsNullOrEmpty(bl.de) &&
                            DateTime.TryParseExact(bl.de, "yyyy-MM-dd",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out dataDe))
                        {
                            cmdBloq.Parameters.AddWithValue("@data_de", dataDe);
                        }
                        else
                        {
                            cmdBloq.Parameters.AddWithValue("@data_de", DBNull.Value);
                        }

                        if (!string.IsNullOrEmpty(bl.ate) &&
                            DateTime.TryParseExact(bl.ate, "yyyy-MM-dd",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out dataAte))
                        {
                            cmdBloq.Parameters.AddWithValue("@data_ate", dataAte);
                        }
                        else
                        {
                            cmdBloq.Parameters.AddWithValue("@data_ate", DBNull.Value);
                        }

                        if (string.IsNullOrEmpty(bl.motivo))
                            cmdBloq.Parameters.AddWithValue("@motivo", DBNull.Value);
                        else
                            cmdBloq.Parameters.AddWithValue("@motivo", bl.motivo);

                        cmdBloq.ExecuteNonQuery();
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
}



//public static List<PreAgendamentoDTO> ListarTodos()
//    {
//        List<PreAgendamentoDTO> lista = new List<PreAgendamentoDTO>();

//        using (SqlConnection con = new SqlConnection(connStr))
//        {
//            string sql = "SELECT * FROM PreAgendamento ORDER BY id DESC";
//            SqlCommand cmd = new SqlCommand(sql, con);

//            con.Open();
//            SqlDataReader dr = cmd.ExecuteReader();

//            while (dr.Read())
//            {
//                lista.Add(new PreAgendamentoDTO
//                {
//                    Id = Convert.ToInt32(dr["id"]),
//                    DataPreenchimento = Convert.ToDateTime(dr["data_preenchimento"]),
//                    Clinica = dr["clinica"].ToString(),
//                    Profissional = dr["profissional"].ToString()
//                });
//            }
//        }

//        return lista;
//    }

//    public static void Excluir(int id)
//    {
//        using (SqlConnection con = new SqlConnection(connStr))
//        {
//            SqlCommand cmd = new SqlCommand("DELETE FROM PreAgendamento WHERE id=@id", con);
//            cmd.Parameters.AddWithValue("@id", id);
//            con.Open();
//            cmd.ExecuteNonQuery();
//        }
//    }
//    public static List<BlocoDiaDTO> ListarDiasPorPreAgendamento(int idPre)
//    {
//        List<BlocoDiaDTO> lista = new List<BlocoDiaDTO>();

//        using (SqlConnection con = new SqlConnection(connStr))
//        {
//            string sql = @"
//SELECT dia_semana, horario, consultas_novas, consultas_retorno, subespecialidade
//FROM PreAgendamentoDia
//WHERE id_preagendamento = @id
//ORDER BY id";

//            SqlCommand cmd = new SqlCommand(sql, con);
//            cmd.Parameters.AddWithValue("@id", idPre);

//            con.Open();
//            SqlDataReader dr = cmd.ExecuteReader();

//            while (dr.Read())
//            {
//                BlocoDiaDTO b = new BlocoDiaDTO();
//                b.diaSemana = dr["dia_semana"].ToString();
//                b.horario = dr["horario"].ToString();
//                b.consultasNovas = dr["consultas_novas"].ToString();
//                b.consultasRetorno = dr["consultas_retorno"].ToString();
//                b.subespecialidade = dr["subespecialidade"].ToString();

//                lista.Add(b);
//            }
//        }

//        return lista;
//    }


