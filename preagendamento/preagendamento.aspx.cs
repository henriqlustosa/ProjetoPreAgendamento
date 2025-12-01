using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;



public partial class publico_preagendamento : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            txtDataPreenchimento.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtDataPreenchimento.Attributes["readonly"] = "readonly";

            CarregarClinicas();

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

    // --- NOVO MÉTODO PARA CARREGAR OS MOTIVOS DO BANCO ---
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
                    ddlClinica.Items.Add(new System.Web.UI.WebControls.ListItem(dr["nm_especialidade"].ToString(), dr["cod_especialidade"].ToString()));
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

        // 1. BLOCOS
        List<BlocoDiaDTO> blocos = PreAgendamentoDAO.ListarBlocos(id) ?? new List<BlocoDiaDTO>();
        hdnBlocosJson.Value = js.Serialize(blocos);

        // 2. BLOQUEIOS (ATUALIZADO)
        // Atenção: Seu DAO.ListarBloqueios deve retornar o 'CodMotivo' agora, não apenas a string.
        List<BloqueioDTO> bloqueios = PreAgendamentoDAO.ListarBloqueios(id) ?? new List<BloqueioDTO>();

        var bloqueiosFormatados = bloqueios.Select(b => new
        {
            De = Convert.ToDateTime(b.de).ToString("yyyy-MM-dd"),
            Ate = Convert.ToDateTime(b.ate).ToString("yyyy-MM-dd"),
            CodMotivo = b.codMotivo // Agora enviamos o ID (int) para o frontend selecionar no dropdown
        }).ToList();

        hdnBloqueiosJson.Value = js.Serialize(bloqueiosFormatados);

        // 3. MESES
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
        if (ddlClinica.Items.Count > 0) ddlClinica.SelectedIndex = 0;

        hdnCodProfissional.Value = string.Empty;
        hdnNomeProfissional.Value = string.Empty;
        hdnBlocosJson.Value = string.Empty;
        hdnBloqueiosJson.Value = string.Empty;
        hdnMesesSelecionados.Value = string.Empty;
        hdnIdPreAgendamento.Value = string.Empty;

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

    protected void btnLimpar_Click(object sender, EventArgs e)
    {
        LimparCampos();
    }

    private List<PeriodoPreAgendamentoDTO> ObterPeriodosSelecionados()
    {
        List<PeriodoPreAgendamentoDTO> periodos = new List<PeriodoPreAgendamentoDTO>();
        string valor = hdnMesesSelecionados.Value;
        if (string.IsNullOrEmpty(valor)) return periodos;

        string[] partes = valor.Split(',');
        foreach (string item in partes)
        {
            string mesTrim = item.Trim();
            if (mesTrim.Length != 7) continue;
            string[] anoMes = mesTrim.Split('-');
            if (anoMes.Length != 2) continue;
            int ano, mes;
            if (int.TryParse(anoMes[0], out ano) && int.TryParse(anoMes[1], out mes))
            {
                periodos.Add(new PeriodoPreAgendamentoDTO { Ano = ano, Mes = mes });
            }
        }
        return periodos;
    }

    protected void btnSalvar_Click(object sender, EventArgs e)
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        List<BlocoDiaDTO> blocos = new List<BlocoDiaDTO>();
        if (!string.IsNullOrEmpty(hdnBlocosJson.Value))
        {
            try { blocos = serializer.Deserialize<List<BlocoDiaDTO>>(hdnBlocosJson.Value); } catch { }
        }

        List<BloqueioDTO> bloqueios = new List<BloqueioDTO>();
        if (!string.IsNullOrEmpty(hdnBloqueiosJson.Value))
        {
            try { bloqueios = serializer.Deserialize<List<BloqueioDTO>>(hdnBloqueiosJson.Value); } catch { }
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
            ScriptManager.RegisterStartupScript(this, GetType(), "errEsp", "alert('Selecione uma clínica/especialidade válida.');", true);
            return;
        }

        dto.CodEspecialidade = codEspecialidade;
        dto.Clinica = ddlClinica.SelectedItem.Text;

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

        int idExistente;
        bool modoEdicao = int.TryParse(hdnIdPreAgendamento.Value, out idExistente) && idExistente > 0;
        if (modoEdicao) dto.Id = idExistente;

        try
        {
            int idGeradoOuAtualizado;
            if (modoEdicao)
            {
                // Recupera usuário logado
                string usuarioLogado = Session["Login"] != null ? Session["Login"].ToString() : "Desconhecido";
                // ATENÇÃO: Verifique se seu método DAO.Atualizar aceita os DTOs com CodMotivo (int)
                PreAgendamentoDAO.Atualizar(dto, blocos, bloqueios, periodos, usuarioLogado);
                idGeradoOuAtualizado = idExistente;
            }
            else
            {
                // ATENÇÃO: Verifique se seu método DAO.Inserir aceita os DTOs com CodMotivo (int)
                idGeradoOuAtualizado = PreAgendamentoDAO.Inserir(dto, blocos, bloqueios);
                if (periodos != null && periodos.Count > 0)
                    PreAgendamentoDAO.InserirPeriodos(idGeradoOuAtualizado, periodos);
            }
            LimparCampos();
            string scriptOk = string.Format("alert('Registro {0} com sucesso! ID: {1}');", modoEdicao ? "atualizado" : "cadastrado", idGeradoOuAtualizado);
            ScriptManager.RegisterStartupScript(this, GetType(), "ok", scriptOk, true);
        }
        catch (Exception ex)
        {
            string msg = ex.Message.Replace("'", "\\'");
            ScriptManager.RegisterStartupScript(this, GetType(), "err", "alert('Erro ao salvar: " + msg + "');", true);
        }
    }
}