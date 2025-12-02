using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq; // C# 3.0 permite LINQ, mas vamos evitar usos complexos
using System.Web.Script.Serialization;
using System.Web.UI;

public partial class visualizar_preagendamento : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
                CarregarClinicas();

            if (Request.QueryString["id"] != null)
            {
                int id;
                if (int.TryParse(Request.QueryString["id"], out id))
                {
                    CarregarRegistroParaVisualizacao(id);
                }
            }
            else
            {
                Response.Redirect("chefia_preagendamento.aspx");
            }
        }
    }

    // --- MÉTODOS ESTÁTICOS ---

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
                    lista.Add(new MotivoBloqueioDTO { CodMotivo = Convert.ToInt32(dr["cod_motivo"]), NmMotivo = dr["nm_motivo"].ToString() });
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

    // --- MÉTODOS DE CARGA ---

    private void CarregarClinicas()
    {
        ddlClinica.Items.Clear();
        string cs = ConfigurationManager.ConnectionStrings["gtaConnectionString"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(cs))
        using (SqlCommand cmd = new SqlCommand("SELECT cod_especialidade, nm_especialidade FROM dbo.Especialidade WHERE status = 'A' ORDER BY nm_especialidade;", conn))
        {
            conn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read()) ddlClinica.Items.Add(new System.Web.UI.WebControls.ListItem(dr["nm_especialidade"].ToString(), dr["cod_especialidade"].ToString()));
            }
        }
    }

    private void CarregarRegistroParaVisualizacao(int id)
    {
        PreAgendamentoDTO dto = PreAgendamentoDAO.ObterPorId(id);
        if (dto == null) { Response.Redirect("chefia_preagendamento.aspx"); return; }

        // Dados Gerais
        hdnIdPreAgendamento.Value = dto.Id.ToString();
        txtDataPreenchimento.Text = dto.DataPreenchimento.ToString("yyyy-MM-dd");

        if (dto.CodEspecialidade > 0 && ddlClinica.Items.FindByValue(dto.CodEspecialidade.ToString()) != null)
            ddlClinica.SelectedValue = dto.CodEspecialidade.ToString();

        hdnCodProfissional.Value = dto.CodProfissional.ToString();
        hdnNomeProfissional.Value = dto.Profissional;
        txtObservacoes.Text = dto.Observacoes ?? string.Empty;

        JavaScriptSerializer js = new JavaScriptSerializer();

        // 1. Blocos
        List<BlocoDiaDTO> blocos = PreAgendamentoDAO.ListarBlocos(id);
        if (blocos == null) blocos = new List<BlocoDiaDTO>();
        hdnBlocosJson.Value = js.Serialize(blocos);

        // 2. Bloqueios (Ajustado para C# 3.0 - substituindo LINQ Select por foreach)
        List<BloqueioDTO> bloqueios = PreAgendamentoDAO.ListarBloqueios(id);
        if (bloqueios == null) bloqueios = new List<BloqueioDTO>();

        // Usando List<object> para garantir compatibilidade total na serialização
        List<object> bloqueiosFormatados = new List<object>();
        foreach (BloqueioDTO b in bloqueios)
        {
            // Objeto anônimo é suportado em C# 3.0
            bloqueiosFormatados.Add(new
            {
                De = Convert.ToDateTime(b.De).ToString("yyyy-MM-dd"),
                Ate = Convert.ToDateTime(b.Ate).ToString("yyyy-MM-dd"),
                CodMotivo = b.CodMotivo
            });
        }
        hdnBloqueiosJson.Value = js.Serialize(bloqueiosFormatados);

        // 3. Meses
        List<PeriodoPreAgendamentoDTO> periodos = PreAgendamentoDAO.ListarPeriodos(id);
        if (periodos == null) periodos = new List<PeriodoPreAgendamentoDTO>();

        List<string> listaMeses = new List<string>();
        foreach (var p in periodos)
        {
            listaMeses.Add(p.Ano + "-" + p.Mes.ToString("00"));
        }
        hdnMesesSelecionados.Value = string.Join(",", listaMeses.ToArray());
    }

    // --- AÇÕES ---

    protected void btnAprovar_Click(object sender, EventArgs e)
    {
        try
        {
            int id = Convert.ToInt32(hdnIdPreAgendamento.Value);
            string usuario = Session["login"] != null ? Session["login"].ToString() : "sistema";

            PreAgendamentoDAO.Aprovar(id, usuario);

            string script = "alert('Pré-agendamento APROVADO com sucesso!'); window.location='chefia_preagendamento.aspx';";
            ScriptManager.RegisterStartupScript(this, GetType(), "aprovado", script, true);
        }
        catch (Exception ex)
        {
            string msg = ex.Message.Replace("'", "");
            // CORREÇÃO C# 3.0: Substituído $"..." por string.Format
            string scriptErro = string.Format("alert('Erro ao aprovar: {0}');", msg);

            ScriptManager.RegisterStartupScript(this, GetType(), "erro", scriptErro, true);
        }
    }

    protected void btnVoltar_Click(object sender, EventArgs e)
    {
        Response.Redirect("chefia_preagendamento.aspx");
    }
}