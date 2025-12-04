using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

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

            CarregarListaHistorico();
        }
    }

    private void CarregarListaHistorico()
    {
        // Chama o método do DAO que agora retorna tanto 'V' (Aprovado) quanto 'R' (Reprovado)
        DataTable dt = PreAgendamentoDAO.ListarAprovadosPorClinica();

        DataView dv = new DataView(dt);

        rptAgendamentos.DataSource = dv;
        rptAgendamentos.DataBind();

        // Controla a visibilidade da tabela ou mensagem de vazio
        divEmpty.Visible = (dv.Count == 0);
        pnlTabela.Visible = (dv.Count > 0);
    }

    protected void rptAgendamentos_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "Visualizar")
        {
            string id = e.CommandArgument.ToString();

            // Passa o parâmetro &origem=aprovados para que a tela de visualização entre em modo "apenas leitura"
            string url = "visualizar_preagendamento.aspx?id=" + Server.UrlEncode(id) + "&origem=aprovados";

            Response.Redirect(url, false);
        }
    }

    // --- MÉTODOS AUXILIARES DE ESTILO (Chamados pelo ASPX) ---

    // Define a classe CSS do Badge baseado no status (Verde ou Vermelho)
    protected string ObterClasseStatus(object statusObj)
    {
        string status = statusObj != null ? statusObj.ToString() : "";

        // Classes base do Tailwind para o badge
        string baseClass = "inline-flex items-center px-3 py-1 rounded-full text-xs font-medium border ";

        if (status == "R")
        {
            // Vermelho para Reprovado
            return baseClass + "bg-red-100 text-red-800 border-red-200";
        }
        else
        {
            // Verde para Aprovado (V)
            return baseClass + "bg-green-100 text-green-800 border-green-200";
        }
    }

    // Define o texto amigável exibido na tela
    protected string ObterTextoStatus(object statusObj)
    {
        string status = statusObj != null ? statusObj.ToString() : "";

        if (status == "R") return "Reprovado";
        if (status == "V") return "Aprovado";

        return status; // Retorna o próprio código se não for nem V nem R (fallback)
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
        int idPerfilChefe = 1; // Ajuste conforme o ID real do perfil
        return (perfis != null && perfis.Contains(idPerfilChefe));
    }
}