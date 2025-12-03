using System;
using System.Collections; // Necessário para ICollection
using System.Web.UI.WebControls;
using System.Data; // Caso o DAO retorne DataTable

public partial class publico_preagendamento_listagem : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CarregarLista();
        }
    }

    private void CarregarLista()
    {
        // "var" funciona no C# 3.0, mas para garantir compatibilidade total
        // e clareza em versões antigas, estou usando "object".
        object dados = PreAgendamentoDAO.ListarTodos();

        // Vincula ao Repeater (frontend)
        rptLista.DataSource = dados;
        rptLista.DataBind();

        // Verifica se a lista está vazia
        bool estaVazio = false;

        // --- ADAPTAÇÃO C# 3.0 ---
        // O Pattern Matching (is Type variable) não existia.
        // Usamos o operador 'as' para tentar converter e checamos null.

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
        // -------------------------

        // Controla a visibilidade dos painéis
        if (estaVazio)
        {
            divEmpty.Visible = true;   // Mostra mensagem de vazio
            pnlTabela.Visible = false; // Esconde a tabela
        }
        else
        {
            divEmpty.Visible = false;  // Esconde mensagem
            pnlTabela.Visible = true;  // Mostra a tabela
        }
    }

    // Evento disparado pelos botões de ação dentro do Repeater
    protected void rptLista_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "Excluir")
        {
            // Pega o ID passado no CommandArgument
            int id = Convert.ToInt32(e.CommandArgument);

            // Identifica o usuário para log de auditoria
            string usuarioLogado = string.Empty;

            if (Session["Login"] != null)
            {
                usuarioLogado = Session["Login"].ToString();
            }
            else
            {
                // Operador ternário é compatível com C# 3.0
                usuarioLogado = User.Identity.IsAuthenticated ? User.Identity.Name : "Sistema";
            }

            // Chama a exclusão no banco
            PreAgendamentoDAO.Excluir(id, usuarioLogado);

            // Recarrega a lista para atualizar a tela
            CarregarLista();
        }
    }
}