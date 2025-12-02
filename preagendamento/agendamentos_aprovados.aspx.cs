using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
// ... demais usings

public partial class agendamentos_aprovados : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (!UsuarioPerfilAdministrador())
            {
                Response.Redirect("~/AcessoNegado.aspx");
                return;
            }

            CarregarListaAprovados();
        }
    }

    private void CarregarListaAprovados()
    {
        DataTable dt = PreAgendamentoDAO.ListarAprovadosPorClinica();

        DataView dv = new DataView(dt);

        try
        {
            dv.RowFilter = "status = 'V'"; // seu filtro atual
        }
        catch
        {
            lblMensagem.Text = "Erro ao filtrar status. Verifique se a coluna 'status' existe no retorno do DAO.";
        }

        rptAgendamentos.DataSource = dv;
        rptAgendamentos.DataBind();

        divEmpty.Visible = (dv.Count == 0);
        pnlTabela.Visible = (dv.Count > 0);
    }

    // NOVO: trata o clique no botão Visualizar
    protected void rptAgendamentos_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "Visualizar")
        {
            string id = e.CommandArgument.ToString();

            // Monta a URL da página de visualização
            // Ajuste o nome do parâmetro conforme o que vc espera em visualizar_preagendamento
            string url = "~/Preagendamento/visualizar_preagendamento.aspx?id=" + Server.UrlEncode(id);

            Response.Redirect(url, false);
        }
    }

    private string ObterUsuarioLogado()
    {
        if (Session["Login"] != null) return Session["Login"].ToString();
        if (User.Identity.IsAuthenticated) return User.Identity.Name;
        return "";
    }

    private bool UsuarioPerfilAdministrador()
    {
        var perfis = Session["perfis"] as List<int>;
        int idPerfilChefe = 1;
        return (perfis != null && perfis.Contains(idPerfilChefe));
    }
}
