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
          //  PreAgendamentoDAO.Excluir(id);
            CarregarLista();
        }
    }
}
