using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Script.Serialization;
using System.Web.UI;

public partial class publico_preagendamento : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // input type="date" => formato yyyy-MM-dd
            txtDataPreenchimento.Text = DateTime.Now.ToString("yyyy-MM-dd");

            // Impede edição manual
            txtDataPreenchimento.Attributes["readonly"] = "readonly";

            CarregarClinicas();

            // REMOVIDO: ddlProfissional.Items.Clear();
            // O controle agora é HTML puro e será gerenciado via JavaScript/HiddenFields

            // --- MODO EDIÇÃO ---
            int idEdicao;
            if (int.TryParse(Request.QueryString["id"], out idEdicao) && idEdicao > 0)
            {
                hdnIdPreAgendamento.Value = idEdicao.ToString();
                lblTituloPagina.Text = "HSPM Pré-Agendamento - Agendas Médicas (Edição)";
                btnSalvar.Text = "Atualizar";

                CarregarRegistroParaEdicao(idEdicao);
            }
            else
            {
                lblTituloPagina.Text = "HSPM Pré-Agendamento - Agendas Médicas (Cadastro)";
                btnSalvar.Text = "Salvar";
            }
        }
    }

    private void CarregarClinicas()
    {
        ddlClinica.Items.Clear();
        ddlClinica.Items.Add(new System.Web.UI.WebControls.ListItem("Selecione...", ""));

        string cs = ConfigurationManager.ConnectionStrings["gtaConnectionString"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(cs))
        using (SqlCommand cmd = new SqlCommand(@"
            SELECT cod_especialidade, nm_especialidade
            FROM dbo.Especialidade
            WHERE status = 'A'
            ORDER BY nm_especialidade;", conn))
        {
            conn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    ddlClinica.Items.Add(
                        new System.Web.UI.WebControls.ListItem(
                            dr["nm_especialidade"].ToString(),
                            dr["cod_especialidade"].ToString()
                        )
                    );
                }
            }
        }
    }

    /// <summary>
    /// Carrega os dados de um pré-agendamento existente para edição.
    /// </summary>
    private void CarregarRegistroParaEdicao(int id)
    {
        PreAgendamentoDTO dto = PreAgendamentoDAO.ObterPorId(id);
        if (dto == null)
            return;

        // Data
        txtDataPreenchimento.Text = dto.DataPreenchimento.ToString("yyyy-MM-dd");

        // Clínica
        if (dto.CodEspecialidade > 0)
        {
            if (ddlClinica.Items.FindByValue(dto.CodEspecialidade.ToString()) != null)
                ddlClinica.SelectedValue = dto.CodEspecialidade.ToString();
        }

        // --- ALTERADO PARA HIDDEN FIELDS ---
        // Em vez de preencher o DropDownList (que não existe mais no server),
        // preenchemos os HiddenFields. O JavaScript no frontend lerá esses valores
        // e selecionará o profissional visualmente.

        hdnCodProfissional.Value = dto.CodProfissional.ToString();
        hdnNomeProfissional.Value = dto.Profissional; // Garanta que o DTO traga o nome

        // Observações
        txtObservacoes.Text = dto.Observacoes ?? string.Empty;

        // Carrega os dados complexos para os HiddenFields JSON
        // O JavaScript (carregarDadosEdicao) vai ler isso e montar a tela
        JavaScriptSerializer js = new JavaScriptSerializer();

        // Blocos
        List<BlocoDiaDTO> blocos = PreAgendamentoDAO.ListarBlocos(id); // Precisa existir no DAO
        hdnBlocosJson.Value = js.Serialize(blocos);

        // Bloqueios
        List<BloqueioDTO> bloqueios = PreAgendamentoDAO.ListarBloqueios(id); // Precisa existir no DAO
        hdnBloqueiosJson.Value = js.Serialize(bloqueios);

        // Meses (Períodos)
        List<PeriodoPreAgendamentoDTO> periodos = PreAgendamentoDAO.ListarPeriodos(id); // Precisa existir no DAO
        List<string> listaMeses = new List<string>();
        foreach (var p in periodos)
        {
            listaMeses.Add(p.Ano + "-" + p.Mes.ToString("00"));
        }
        hdnMesesSelecionados.Value = string.Join(",", listaMeses.ToArray());
    }

    // ---------------------------------------------------------
    // MÉTODO PARA LIMPAR OS CAMPOS
    // ---------------------------------------------------------
    private void LimparCampos()
    {
        txtObservacoes.Text = string.Empty;

        if (ddlClinica.Items.Count > 0)
            ddlClinica.SelectedIndex = 0;

        // --- ALTERADO: Limpa HiddenFields em vez de DropDownList ---
        hdnCodProfissional.Value = string.Empty;
        hdnNomeProfissional.Value = string.Empty;

        hdnBlocosJson.Value = string.Empty;
        hdnBloqueiosJson.Value = string.Empty;
        hdnMesesSelecionados.Value = string.Empty;
        hdnIdPreAgendamento.Value = string.Empty;

        // Mantém sempre a data atual no formato do input date
        txtDataPreenchimento.Text = DateTime.Now.ToString("yyyy-MM-dd");

        lblTituloPagina.Text = "HSPM Pré-Agendamento - Agendas Médicas (Cadastro)";
        btnSalvar.Text = "Salvar";
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
            WHERE status = 'A'
            AND cod_especialidade = @cod
            ORDER BY nm_subespecialidade;", conn))
        {
            cmd.Parameters.AddWithValue("@cod", codEspecialidade);
            conn.Open();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SubespecialidadeDTO dto = new SubespecialidadeDTO();
                    dto.CodSubespecialidade = Convert.ToInt32(dr["cod_subespecialidade"]);
                    dto.NomeSubespecialidade = dr["nm_subespecialidade"].ToString();
                    lista.Add(dto);
                }
            }
        }
        return lista;
    }

    // WebMethod para carregar profissionais da clínica selecionada (chamado via PageMethods)
    [System.Web.Services.WebMethod]
    public static List<ProfissionalDTO> ListarProfissionais(int codEspecialidade)
    {
        return PreAgendamentoDAO.ListarProfissionaisPorEspecialidade(codEspecialidade);
    }

    protected void btnLimpar_Click(object sender, EventArgs e)
    {
        LimparCampos();
    }

    // --------------------------------------------------------------------
    // Lê o HiddenField de meses e devolve uma lista tipada (Ano / Mês)
    // --------------------------------------------------------------------
    private List<PeriodoPreAgendamentoDTO> ObterPeriodosSelecionados()
    {
        List<PeriodoPreAgendamentoDTO> periodos = new List<PeriodoPreAgendamentoDTO>();

        string valor = hdnMesesSelecionados.Value;
        if (string.IsNullOrEmpty(valor))
            return periodos;

        string[] partes = valor.Split(',');
        foreach (string item in partes)
        {
            string mesTrim = item.Trim();
            if (mesTrim.Length != 7) continue;

            string[] anoMes = mesTrim.Split('-');
            if (anoMes.Length != 2) continue;

            int ano;
            int mes;

            if (int.TryParse(anoMes[0], out ano) &&
                int.TryParse(anoMes[1], out mes))
            {
                PeriodoPreAgendamentoDTO p = new PeriodoPreAgendamentoDTO();
                p.Ano = ano;
                p.Mes = mes;
                periodos.Add(p);
            }
        }

        return periodos;
    }

    protected void btnSalvar_Click(object sender, EventArgs e)
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // 1) Ler os blocos de dia da semana
        List<BlocoDiaDTO> blocos = new List<BlocoDiaDTO>();
        if (!string.IsNullOrEmpty(hdnBlocosJson.Value))
        {
            try { blocos = serializer.Deserialize<List<BlocoDiaDTO>>(hdnBlocosJson.Value); }
            catch { blocos = new List<BlocoDiaDTO>(); }
        }

        // 2) Ler os bloqueios
        List<BloqueioDTO> bloqueios = new List<BloqueioDTO>();
        if (!string.IsNullOrEmpty(hdnBloqueiosJson.Value))
        {
            try { bloqueios = serializer.Deserialize<List<BloqueioDTO>>(hdnBloqueiosJson.Value); }
            catch { bloqueios = new List<BloqueioDTO>(); }
        }

        // 3) Ler os períodos
        List<PeriodoPreAgendamentoDTO> periodos = ObterPeriodosSelecionados();

        // 4) Montar o DTO principal
        PreAgendamentoDTO dto = new PreAgendamentoDTO();

        DateTime dataPreenchimento;
        if (!DateTime.TryParseExact(
                txtDataPreenchimento.Text,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dataPreenchimento))
        {
            dataPreenchimento = DateTime.Now;
        }

        dto.DataPreenchimento = dataPreenchimento;

        // Clínica (obrigatório)
        int codEspecialidade;
        if (!int.TryParse(ddlClinica.SelectedValue, out codEspecialidade) || codEspecialidade <= 0)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "errEsp", "alert('Selecione uma clínica/especialidade válida.');", true);
            return;
        }

        dto.CodEspecialidade = codEspecialidade;
        dto.Clinica = ddlClinica.SelectedItem.Text;

        // --- ALTERADO: Profissional (LÊ DO HIDDEN FIELD) ---
        int codProfissional;
        if (!int.TryParse(hdnCodProfissional.Value, out codProfissional) || codProfissional <= 0)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "errProf", "alert('Selecione um profissional.');", true);
            return;
        }

        dto.CodProfissional = codProfissional;
        dto.Profissional = hdnNomeProfissional.Value; 

        dto.Observacoes = txtObservacoes.Text;
        dto.Usuario = (Session["login"] ?? "desconhecido").ToString();
        dto.DataCadastro = DateTime.Now;

        // Verifica se é inclusão ou edição
        int idExistente;
        bool modoEdicao = int.TryParse(hdnIdPreAgendamento.Value, out idExistente) && idExistente > 0;
        if (modoEdicao)
        {
            dto.Id = idExistente; // Assumindo que seu DTO tem essa propriedade
        }

        try
        {
            int idGeradoOuAtualizado;

            if (modoEdicao)
            {
                // Atualiza (Método Atualizar precisa existir no DAO)
                // Geralmente: Atualiza pai, deleta filhos antigos, insere novos
                PreAgendamentoDAO.Atualizar(dto, blocos, bloqueios, periodos);
                idGeradoOuAtualizado = idExistente;
            }
            else
            {
                // Inserir novo
                idGeradoOuAtualizado = PreAgendamentoDAO.Inserir(dto, blocos, bloqueios);

                if (periodos != null && periodos.Count > 0)
                {
                    PreAgendamentoDAO.InserirPeriodos(idGeradoOuAtualizado, periodos);
                }
            }

            LimparCampos();

            string scriptOk = string.Format(
                "alert('Registro {0} com sucesso! ID: {1}');",
                modoEdicao ? "atualizado" : "cadastrado",
                idGeradoOuAtualizado);

            ScriptManager.RegisterStartupScript(this, GetType(), "ok", scriptOk, true);
        }
        catch (Exception ex)
        {
            string msg = ex.Message.Replace("'", "\\'");
            string script = string.Format("alert('Erro ao salvar: {0}');", msg);
            ScriptManager.RegisterStartupScript(this, GetType(), "err", script, true);
        }
    }
}