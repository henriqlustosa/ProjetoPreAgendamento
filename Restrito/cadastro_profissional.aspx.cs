using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class cadastro_profissional : System.Web.UI.Page
{
    // Propriedade para controlar o índice da página atual no ViewState
    public int PageIndex
    {
        get
        {
            if (ViewState["PageIndex"] == null)
                return 0;
            return (int)ViewState["PageIndex"];
        }
        set
        {
            ViewState["PageIndex"] = value;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CarregarEspecialidades();
            CarregarListaProfissionais();
        }
    }

    private void CarregarEspecialidades()
    {
        try
        {
            DataTable dt = ProfissionalDAO.ListarEspecialidadesAtivas();
            ddlEspecialidade.DataSource = dt;
            ddlEspecialidade.DataTextField = "nm_especialidade";
            ddlEspecialidade.DataValueField = "cod_especialidade";
            ddlEspecialidade.DataBind();
            ddlEspecialidade.Items.Insert(0, new ListItem("-- Selecione --", "0"));
        }
        catch (Exception ex)
        {
            ExibirMensagem("Erro ao carregar especialidades: " + ex.Message, false);
        }
    }

    private void CarregarListaProfissionais()
    {
        try
        {
            DataTable dt = ProfissionalDAO.ListarProfissionaisCompleto();
            DataView dv = dt.DefaultView;

            // LÓGICA DE BUSCA SERVER-SIDE
            // Aplica o filtro sobre o DataView se houver texto na busca
            if (!string.IsNullOrEmpty(txtBusca.Text.Trim()))
            {
                // Filtra pelo nome (LIKE %texto%)
                // Replace("'", "''") previne erros de sintaxe no RowFilter
                dv.RowFilter = "nome_profissional LIKE '%" + txtBusca.Text.Trim().Replace("'", "''") + "%'";
            }

            // Configuração da Paginação
            PagedDataSource pds = new PagedDataSource();
            pds.DataSource = dv; // Usa o DataView filtrado
            pds.AllowPaging = true;
            pds.PageSize = 20; // 20 registros por página
            pds.CurrentPageIndex = PageIndex;

            // Controle dos Botões
            btnAnt.Enabled = !pds.IsFirstPage;
            btnProx.Enabled = !pds.IsLastPage;

            // Estilização visual dos botões desabilitados
            btnAnt.CssClass = pds.IsFirstPage ? "px-3 py-1 bg-gray-100 border border-gray-300 rounded text-sm font-medium text-gray-400 cursor-not-allowed" : "px-3 py-1 bg-white border border-gray-300 rounded text-sm font-medium text-gray-700 hover:bg-gray-50 cursor-pointer";
            btnProx.CssClass = pds.IsLastPage ? "px-3 py-1 bg-gray-100 border border-gray-300 rounded text-sm font-medium text-gray-400 cursor-not-allowed" : "px-3 py-1 bg-white border border-gray-300 rounded text-sm font-medium text-gray-700 hover:bg-gray-50 cursor-pointer";

            // Informação da página
            int totalPaginas = pds.PageCount > 0 ? pds.PageCount : 1;
            lblPageInfo.Text = (PageIndex + 1) + " de " + totalPaginas;

            rptProfissionais.DataSource = pds;
            rptProfissionais.DataBind();

            // Mostra mensagem vazia se não houver registros após o filtro
            divEmpty.Visible = (pds.Count == 0);
        }
        catch (Exception ex)
        {
            ExibirMensagem("Erro ao listar profissionais: " + ex.Message, false);
        }
    }

    // Evento do Botão/Enter na Busca
    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        // Reseta para a primeira página ao fazer uma nova busca
        PageIndex = 0;
        CarregarListaProfissionais();
    }

    protected void btnAnt_Click(object sender, EventArgs e)
    {
        PageIndex -= 1;
        CarregarListaProfissionais();
    }

    protected void btnProx_Click(object sender, EventArgs e)
    {
        PageIndex += 1;
        CarregarListaProfissionais();
    }

    protected void btnSalvar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        try
        {
            string nome = txtNome.Text.Trim();
            int codEspecialidade = Convert.ToInt32(ddlEspecialidade.SelectedValue);
            bool ativo = chkAtivo.Checked;
            bool chefe = chkChefe.Checked;
            string andar = txtAndar.Text.Trim();

            bool sucesso = ProfissionalDAO.CadastrarProfissional(nome, ativo, codEspecialidade, chefe, andar);

            if (sucesso)
            {
                ExibirMensagem("Profissional cadastrado com sucesso!", true);
                LimparFormulario();
                PageIndex = 0; // Volta para a primeira página ao inserir
                txtBusca.Text = string.Empty; // Limpa a busca para mostrar o novo registro
                CarregarListaProfissionais();
            }
            else
            {
                ExibirMensagem("Erro ao cadastrar. Verifique os dados.", false);
            }
        }
        catch (Exception ex)
        {
            ExibirMensagem("Erro técnico: " + ex.Message, false);
        }
    }

    protected void rptProfissionais_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "Excluir")
        {
            try
            {
                int idProf = Convert.ToInt32(e.CommandArgument);
                ProfissionalDAO.InativarProfissional(idProf);
                ExibirMensagem("Profissional inativado.", true);
                CarregarListaProfissionais();
            }
            catch (Exception ex)
            {
                ExibirMensagem("Erro ao excluir: " + ex.Message, false);
            }
        }
    }

    private void LimparFormulario()
    {
        txtNome.Text = string.Empty;
        txtAndar.Text = string.Empty;
        ddlEspecialidade.SelectedIndex = 0;
        chkChefe.Checked = false;
        chkAtivo.Checked = true;
    }

    private void ExibirMensagem(string msg, bool sucesso)
    {
        lblMensagem.Text = msg;
        lblMensagem.Visible = true;

        if (sucesso)
            lblMensagem.CssClass = "block mb-4 p-4 rounded-lg text-center font-bold shadow-sm bg-green-100 text-green-800 border border-green-200";
        else
            lblMensagem.CssClass = "block mb-4 p-4 rounded-lg text-center font-bold shadow-sm bg-red-100 text-red-800 border border-red-200";
    }
}