using System;
using System.Web.UI.WebControls;

public partial class publico_preagendamento_listagem : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            CarregarLista();
    }

    private void CarregarLista()
    {
        gvLista.DataSource = PreAgendamentoDAO.ListarTodos();
        gvLista.DataBind();
    }

    protected void gvLista_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Excluir")
        {
            int id = Convert.ToInt32(e.CommandArgument);

            // 1. Recupera o nome do usuário. 
            // IMPORTANTE: Troque "Login" pela chave correta da sua Sessão (ex: "Usuario", "User", "Matricula")
            string usuarioLogado = string.Empty;

            if (Session["Login"] != null)
            {
                usuarioLogado = Session["Login"].ToString();
            }
            else
            {
                // Caso a sessão tenha caído ou não exista, define um valor padrão ou pega do contexto do Windows/Forms
                usuarioLogado = User.Identity.IsAuthenticated ? User.Identity.Name : "Desconhecido";
            }

            // 2. Passa o ID e o Usuário para o método atualizado no DAO
            PreAgendamentoDAO.Excluir(id, usuarioLogado);

            CarregarLista();
        }
    }
}
