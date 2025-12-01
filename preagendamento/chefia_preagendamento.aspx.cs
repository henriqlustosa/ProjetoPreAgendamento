using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class chefia_preagendamento : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // 1. Verificação de Segurança
            if (!UsuarioEhChefe())
            {
                // Se não for chefe, redireciona para acesso negado ou login
                Response.Redirect("~/AcessoNegado.aspx");
                return;
            }

            CarregarListaChefia();
        }
    }

    private void CarregarListaChefia()
    {
        string usuarioLogado = ObterUsuarioLogado();

        // 2. Descobre qual clínica (especialidade) esse chefe comanda
        int codEspecialidade = ObterEspecialidadeDoChefe(usuarioLogado);

        if (codEspecialidade > 0)
        {
            // 3. Chama o DAO filtrado
            gvLista.DataSource = PreAgendamentoDAO.ListarPorClinica(codEspecialidade);
            gvLista.DataBind();

            // Mostra mensagem se vazio
            divEmpty.Visible = (gvLista.Rows.Count == 0);
        }
        else
        {
            // Caso o usuário seja chefe mas não tenha clínica vinculada no cadastro
            lblMensagem.Text = "Seu usuário não tem uma clínica vinculada.";
            divEmpty.Visible = true;
        }
    }

    // --- MÉTODOS AUXILIARES (Ajuste conforme sua arquitetura de Login) ---

    private string ObterUsuarioLogado()
    {
        if (Session["Login"] != null) return Session["Login"].ToString();
        if (User.Identity.IsAuthenticated) return User.Identity.Name;
        return "";
    }

    // --- AQUI ESTÁ A IMPLEMENTAÇÃO SOLICITADA ---
    private bool UsuarioEhChefe()
    {
        // 1. Recupera a lista de IDs de perfis da Sessão (mesma lógica da BasePage)
        var perfis = Session["perfis"] as List<int>;

        // 2. IMPORTANTE: Defina aqui o ID que representa o "Chefe" no seu Banco de Dados
        // Vá na tabela 'Perfis' do seu banco SQL e veja qual é o número (ID) do perfil Chefe.
        int idPerfilChefe = 5; // <--- SUBSTITUA '2' PELO ID REAL DO SEU BANCO

        // 3. Verifica se a lista existe e se contém o ID do chefe
        if (perfis != null && perfis.Contains(idPerfilChefe))
        {
            return true;
        }

        return false;
    }

    // Adicione este método na sua classe (pode ser na BasePage ou na chefia_preagendamento)
    public int ObterEspecialidadeDoChefe(string usuarioLogado)
    {
        int codEspecialidade = 0; // Retorna 0 se não encontrar

        // String de conexão (ajuste o nome se for diferente de "gtaConnectionString")
        string connectionString = ConfigurationManager.ConnectionStrings["gtaConnectionString"].ToString();

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            // A Query faz o seguinte caminho:
            // 1. Pega o usuarioLogado (LoginRede) na tabela Usuarios
            // 2. Liga com a tabela Profissional pelo NOME (u.NomeCompleto = p.nome_profissional)
            // 3. Liga com ProfissionalEspecialidade pelo código do profissional
            // 4. Filtra onde 'chefe = 1' (conforme sua imagem mostra a coluna chefe)

            string sql = @"
            SELECT TOP 1 pe.cod_especialidade
            FROM [hspmPreAgendamento].[dbo].[ProfissionalEspecialidade] pe
            INNER JOIN [hspmPreAgendamento].[dbo].[Profissional] p 
                ON p.cod_profissional = pe.cod_profissional
            INNER JOIN [hspmPreAgendamento].[dbo].[Usuarios] u 
                ON u.NomeCompleto = p.nome_profissional
            WHERE u.LoginRede = @Login 
              AND pe.chefe = 1";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Login", usuarioLogado);

                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        codEspecialidade = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    // Tratamento de erro ou log
                    throw new Exception("Erro ao buscar especialidade do chefe: " + ex.Message);
                }
            }
        }

        return codEspecialidade;
    }

    protected void gvLista_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // Se o chefe também puder excluir, mantenha essa lógica.
        // Se for apenas visualização, remova este método e os botões do ASPX.
        if (e.CommandName == "Excluir")
        {
            int id = Convert.ToInt32(e.CommandArgument);
            PreAgendamentoDAO.Excluir(id, ObterUsuarioLogado());
            CarregarListaChefia();
        }
    }
}