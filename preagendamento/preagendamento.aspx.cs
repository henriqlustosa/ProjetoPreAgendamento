using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq; // Essencial para o C# 3.0 (LINQ)
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class publico_preagendamento : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Define data de hoje
            txtDataPreenchimento.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtDataPreenchimento.Attributes["readonly"] = "readonly";

            CarregarClinicas();

            // Verifica se é edição
            int idEdicao;
            if (int.TryParse(Request.QueryString["id"], out idEdicao) && idEdicao > 0)
            {
                hdnIdPreAgendamento.Value = idEdicao.ToString();
                lblTituloPagina.Text = "Edição de Agenda Médica";
                btnSalvar.Text = "Atualizar Agenda";
                CarregarRegistroParaEdicao(idEdicao);
            }
            else
            {
                lblTituloPagina.Text = "Cadastro de Agenda Médica";
                btnSalvar.Text = "Salvar Agenda";
            }
        }
    }

    // --- MÉTODOS AJAX (Chamados pelo JavaScript via PageMethods) ---

    [System.Web.Services.WebMethod]
    public static List<MotivoBloqueioDTO> ListarMotivosBloqueio()
    {
        List<MotivoBloqueioDTO> lista = new List<MotivoBloqueioDTO>();
        string cs = ConfigurationManager.ConnectionStrings["gtaConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(cs))
        using (SqlCommand cmd = new SqlCommand("SELECT cod_motivo, nm_motivo FROM PreAgendamentoMotivo WHERE status = 'A' ORDER BY nm_motivo", conn))
        {
            conn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new MotivoBloqueioDTO
                    {
                        CodMotivo = Convert.ToInt32(dr["cod_motivo"]),
                        NmMotivo = dr["nm_motivo"].ToString()
                    });
                }
            }
        }
        return lista;
    }

    [System.Web.Services.WebMethod]
    public static List<SubespecialidadeDTO> ListarSubespecialidades(int codEspecialidade)
    {
        List<SubespecialidadeDTO> lista = new List<SubespecialidadeDTO>();
        string cs = ConfigurationManager.ConnectionStrings["gtaConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(cs))
        using (SqlCommand cmd = new SqlCommand(@"
            SELECT cod_subespecialidade, nm_subespecialidade 
            FROM dbo.SubEspecialidade 
            WHERE status = 'A' AND cod_especialidade = @cod 
            ORDER BY nm_subespecialidade;", conn))
        {
            cmd.Parameters.AddWithValue("@cod", codEspecialidade);
            conn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new SubespecialidadeDTO
                    {
                        CodSubespecialidade = Convert.ToInt32(dr["cod_subespecialidade"]),
                        NomeSubespecialidade = dr["nm_subespecialidade"].ToString()
                    });
                }
            }
        }
        return lista;
    }

    [System.Web.Services.WebMethod]
    public static List<ProfissionalDTO> ListarProfissionais(int codEspecialidade)
    {
        return PreAgendamentoDAO.ListarProfissionaisPorEspecialidade(codEspecialidade);
    }

    // --- MÉTODOS DE APOIO ---

    private void CarregarClinicas()
    {
        ddlClinica.Items.Clear();
        ddlClinica.Items.Add(new ListItem("Selecione...", ""));

        string cs = ConfigurationManager.ConnectionStrings["gtaConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(cs))
        using (SqlCommand cmd = new SqlCommand("SELECT cod_especialidade, nm_especialidade FROM dbo.Especialidade WHERE status = 'A' ORDER BY nm_especialidade;", conn))
        {
            conn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    ddlClinica.Items.Add(new ListItem(dr["nm_especialidade"].ToString(), dr["cod_especialidade"].ToString()));
                }
            }
        }
    }

    private void CarregarRegistroParaEdicao(int id)
    {
        PreAgendamentoDTO dto = PreAgendamentoDAO.ObterPorId(id);
        if (dto == null) return;

        txtDataPreenchimento.Text = dto.DataPreenchimento.ToString("yyyy-MM-dd");

        if (dto.CodEspecialidade > 0)
        {
            if (ddlClinica.Items.FindByValue(dto.CodEspecialidade.ToString()) != null)
                ddlClinica.SelectedValue = dto.CodEspecialidade.ToString();
        }

        hdnCodProfissional.Value = dto.CodProfissional.ToString();
        hdnNomeProfissional.Value = dto.Profissional;
        txtObservacoes.Text = dto.Observacoes ?? string.Empty;

        JavaScriptSerializer js = new JavaScriptSerializer();

        // 1. CARREGAR BLOCOS (HORÁRIOS)
        List<BlocoDiaDTO> blocos = PreAgendamentoDAO.ListarBlocos(id) ?? new List<BlocoDiaDTO>();
        hdnBlocosJson.Value = js.Serialize(blocos);

        // 2. CARREGAR BLOQUEIOS
        List<BloqueioDTO> bloqueios = PreAgendamentoDAO.ListarBloqueios(id) ?? new List<BloqueioDTO>();

        // Uso de LINQ e Tipos Anônimos (suportado no C# 3.0)
        var bloqueiosFormatados = bloqueios.Select(b => new
        {
            De = Convert.ToDateTime(b.De).ToString("yyyy-MM-dd"),
            Ate = Convert.ToDateTime(b.Ate).ToString("yyyy-MM-dd"),
            CodMotivo = b.CodMotivo,
            MotivoTexto = b.MotivoTexto
        }).ToList();

        hdnBloqueiosJson.Value = js.Serialize(bloqueiosFormatados);

        // 3. CARREGAR MESES
        List<PeriodoPreAgendamentoDTO> periodos = PreAgendamentoDAO.ListarPeriodos(id) ?? new List<PeriodoPreAgendamentoDTO>();
        List<string> listaMeses = new List<string>();
        foreach (var p in periodos)
        {
            listaMeses.Add(p.Ano + "-" + p.Mes.ToString("00"));
        }
        hdnMesesSelecionados.Value = string.Join(",", listaMeses.ToArray());
    }

    private void LimparCampos()
    {
        txtObservacoes.Text = string.Empty;
        ddlClinica.SelectedIndex = 0;
        hdnCodProfissional.Value = string.Empty;
        hdnNomeProfissional.Value = string.Empty;
        hdnBlocosJson.Value = string.Empty;
        hdnBloqueiosJson.Value = string.Empty;
        hdnMesesSelecionados.Value = string.Empty;
        hdnIdPreAgendamento.Value = string.Empty;

        lblTituloPagina.Text = "Cadastro de Agenda Médica";
        btnSalvar.Text = "Salvar Agenda";
    }

    // --- AÇÃO DE SALVAR ---

    protected void btnSalvar_Click(object sender, EventArgs e)
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        List<BlocoDiaDTO> blocos = new List<BlocoDiaDTO>();
        if (!string.IsNullOrEmpty(hdnBlocosJson.Value))
        {
            try { blocos = serializer.Deserialize<List<BlocoDiaDTO>>(hdnBlocosJson.Value); }
            catch { }
        }

        List<BloqueioDTO> bloqueios = new List<BloqueioDTO>();
        if (!string.IsNullOrEmpty(hdnBloqueiosJson.Value))
        {
            try { bloqueios = serializer.Deserialize<List<BloqueioDTO>>(hdnBloqueiosJson.Value); }
            catch { }
        }

        List<PeriodoPreAgendamentoDTO> periodos = ObterPeriodosSelecionados();
        PreAgendamentoDTO dto = new PreAgendamentoDTO();

        DateTime dataPreenchimento;
        if (!DateTime.TryParseExact(txtDataPreenchimento.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dataPreenchimento))
            dataPreenchimento = DateTime.Now;
        dto.DataPreenchimento = dataPreenchimento;

        int codEspecialidade;
        if (!int.TryParse(ddlClinica.SelectedValue, out codEspecialidade) || codEspecialidade <= 0)
        {
            ExibirAlerta("Selecione uma clínica/especialidade válida.");
            return;
        }
        dto.CodEspecialidade = codEspecialidade;
        dto.Clinica = ddlClinica.SelectedItem.Text;

        int codProfissional;
        if (!int.TryParse(hdnCodProfissional.Value, out codProfissional) || codProfissional <= 0)
        {
            ExibirAlerta("Selecione um profissional válido.");
            return;
        }
        dto.CodProfissional = codProfissional;
        dto.Profissional = hdnNomeProfissional.Value;
        dto.Observacoes = txtObservacoes.Text;
        dto.Usuario = (Session["Login"] ?? "Sistema").ToString();
        dto.DataCadastro = DateTime.Now;

        int idExistente;
        bool modoEdicao = int.TryParse(hdnIdPreAgendamento.Value, out idExistente) && idExistente > 0;
        if (modoEdicao) dto.Id = idExistente;

        try
        {
            int idResultado;
            if (modoEdicao)
            {
                PreAgendamentoDAO.Atualizar(dto, blocos, bloqueios, periodos, dto.Usuario);
                idResultado = idExistente;
                ExibirAlerta("Agenda atualizada com sucesso!");
            }
            else
            {
                idResultado = PreAgendamentoDAO.Inserir(dto, blocos, bloqueios);
                if (periodos.Count > 0)
                    PreAgendamentoDAO.InserirPeriodos(idResultado, periodos);

                ExibirAlerta("Agenda cadastrada com sucesso! ID: " + idResultado);
                LimparCampos();
            }
        }
        catch (Exception ex)
        {
            ExibirAlerta("Erro ao salvar: " + ex.Message.Replace("'", ""));
        }
    }

    protected void btnLimpar_Click(object sender, EventArgs e)
    {
        LimparCampos();
        Response.Redirect(Request.RawUrl);
    }

    private List<PeriodoPreAgendamentoDTO> ObterPeriodosSelecionados()
    {
        var lista = new List<PeriodoPreAgendamentoDTO>();
        string valor = hdnMesesSelecionados.Value;

        if (string.IsNullOrEmpty(valor)) return lista;

        foreach (string item in valor.Split(','))
        {
            string[] partes = item.Trim().Split('-');
            if (partes.Length == 2)
            {
                int ano, mes;
                if (int.TryParse(partes[0], out ano) && int.TryParse(partes[1], out mes))
                {
                    lista.Add(new PeriodoPreAgendamentoDTO { Ano = ano, Mes = mes });
                }
            }
        }
        return lista;
    }

    private void ExibirAlerta(string msg)
    {
        // AJUSTE C# 3.0: Substituída interpolação de string ($"") por string.Format
        ScriptManager.RegisterStartupScript(this, GetType(), "alert", string.Format("alert('{0}');", msg), true);
    }
}

