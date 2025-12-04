using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public class ProfissionalDAO
{
    private static string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["gtaConnectionString"].ConnectionString; }
    }

    // 1. Listar Especialidades para o DropDown
    public static DataTable ListarEspecialidadesAtivas()
    {
        using (SqlConnection con = new SqlConnection(ConnStr))
        {
            // Filtra apenas status 'A' (Ativo) se existir essa convenção, ou use status=1
            string sql = "SELECT cod_especialidade, nm_especialidade FROM dbo.Especialidade WHERE status = 'A' ORDER BY nm_especialidade";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }

    // 2. Listar Profissionais com suas Especialidades (JOIN)
    public static DataTable ListarProfissionaisCompleto()
    {
        using (SqlConnection con = new SqlConnection(ConnStr))
        {
            // CORREÇÃO: Usamos ISNULL para evitar que valores nulos quebrem o frontend (Convert.ToBoolean)
            // ALTERAÇÃO DE ORDEM: Especialidade -> Chefe (DESC) -> Nome (ASC)
            string sql = @"
                SELECT 
                    p.cod_profissional,
                    p.nome_profissional,
                    p.ativo,
                    ISNULL(e.nm_especialidade, '-') as nm_especialidade,
                    ISNULL(pe.chefe, 0) as chefe,
                    ISNULL(pe.andar, '') as andar
                FROM [hspmPreAgendamento].[dbo].[Profissional] p
                LEFT JOIN [hspmPreAgendamento].[dbo].[ProfissionalEspecialidade] pe 
                    ON p.cod_profissional = pe.cod_profissional
                LEFT JOIN [hspmPreAgendamento].[dbo].[Especialidade] e 
                    ON pe.cod_especialidade = e.cod_especialidade
                ORDER BY 
                    e.nm_especialidade ASC, 
                    ISNULL(pe.chefe, 0) DESC, 
                    p.nome_profissional ASC";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }

    // 3. Cadastrar Profissional e Vínculo (TRANSAÇÃO)
    public static bool CadastrarProfissional(string nome, bool ativo, int codEspecialidade, bool chefe, string andar)
    {
        using (SqlConnection con = new SqlConnection(ConnStr))
        {
            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                // Passo 1: Inserir na tabela Profissional
                string sqlProf = @"
                    INSERT INTO dbo.Profissional (nome_profissional, ativo, data_cadastro) 
                    VALUES (@nome, @ativo, GETDATE());
                    SELECT SCOPE_IDENTITY();"; // Retorna o ID gerado

                int novoIdProfissional = 0;

                using (SqlCommand cmdProf = new SqlCommand(sqlProf, con, tran))
                {
                    cmdProf.Parameters.AddWithValue("@nome", nome);
                    cmdProf.Parameters.AddWithValue("@ativo", ativo);

                    // Executa e pega o ID
                    novoIdProfissional = Convert.ToInt32(cmdProf.ExecuteScalar());
                }

                // Passo 2: Inserir na tabela ProfissionalEspecialidade (Link)
                string sqlLink = @"
                    INSERT INTO dbo.ProfissionalEspecialidade (cod_profissional, cod_especialidade, chefe, andar)
                    VALUES (@codProf, @codEsp, @chefe, @andar)";

                using (SqlCommand cmdLink = new SqlCommand(sqlLink, con, tran))
                {
                    cmdLink.Parameters.AddWithValue("@codProf", novoIdProfissional);
                    cmdLink.Parameters.AddWithValue("@codEsp", codEspecialidade);
                    cmdLink.Parameters.AddWithValue("@chefe", chefe); // Assumindo bit/boolean no banco

                    if (string.IsNullOrEmpty(andar))
                        cmdLink.Parameters.AddWithValue("@andar", DBNull.Value);
                    else
                        cmdLink.Parameters.AddWithValue("@andar", andar);

                    cmdLink.ExecuteNonQuery();
                }

                // Se tudo deu certo, comita a transação
                tran.Commit();
                return true;
            }
            catch (Exception)
            {
                // Se deu erro, desfaz tudo
                tran.Rollback();
                throw; // Repassa o erro para o frontend mostrar
            }
        }
    }

    // 4. Inativar Profissional
    public static void InativarProfissional(int id)
    {
        using (SqlConnection con = new SqlConnection(ConnStr))
        {
            string sql = "UPDATE dbo.Profissional SET ativo = 0 WHERE cod_profissional = @id";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}