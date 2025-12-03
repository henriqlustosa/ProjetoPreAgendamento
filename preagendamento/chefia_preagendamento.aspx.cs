using System;
using System.Collections; // Necessário para ICollection
using System.Collections.Generic; // Para List<int>
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
            object dados = PreAgendamentoDAO.ListarPorClinica(codEspecialidade);

            // Vincula ao Repeater
            rptLista.DataSource = dados;
            rptLista.DataBind();

            // Verificação de Vazio compatível com C# 3.0
            bool estaVazio = false;

            ICollection collection = dados as ICollection;
            DataTable dt = dados as DataTable;

            if (collection != null)
            {
                estaVazio = (collection.Count == 0);
            }
            else if (dt != null)
            {
                estaVazio = (dt.Rows.Count == 0);
            }

            // Controla a visibilidade
            if (estaVazio)
            {
                divEmpty.Visible = true;
                pnlTabela.Visible = false;
            }
            else
            {
                divEmpty.Visible = false;
                pnlTabela.Visible = true;
            }
        }
        else
        {
            lblMensagem.Text = "Seu usuário não tem uma clínica vinculada.";
            divEmpty.Visible = true;
            pnlTabela.Visible = false;
        }
    }

    // --- MÉTODOS AUXILIARES ---

    private string ObterUsuarioLogado()
    {
        if (Session["Login"] != null) return Session["Login"].ToString();
        if (User.Identity.IsAuthenticated) return User.Identity.Name;
        return "";
    }

    private bool UsuarioEhChefe()
    {
        // Cast seguro com 'as' para C# 3.0
        List<int> perfis = Session["perfis"] as List<int>;

        int idPerfilChefe = 5; // ID do perfil Chefe no banco

        if (perfis != null && perfis.Contains(idPerfilChefe))
        {
            return true;
        }

        return false;
    }

    public int ObterEspecialidadeDoChefe(string usuarioLogado)
    {
        int codEspecialidade = 0;

        string connectionString = ConfigurationManager.ConnectionStrings["gtaConnectionString"].ToString();

        using (SqlConnection con = new SqlConnection(connectionString))
        {
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
                    // Logar erro se necessário
                    lblMensagem.Text = "Erro ao identificar especialidade: " + ex.Message;
                }
            }
        }

        return codEspecialidade;
    }

    // ALTERADO: De RowCommand (GridView) para ItemCommand (Repeater)
    protected void rptLista_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "Excluir")
        {
            int id = Convert.ToInt32(e.CommandArgument);
            PreAgendamentoDAO.Excluir(id, ObterUsuarioLogado());

            CarregarListaChefia();
        }
    }
}