using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.UI;

public partial class publico_preagendamento : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // força HTML5 type="date"
            txtDataPreenchimento.Attributes["type"] = "date";

            CarregarClinicas();
            //CarregarMotivos();
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
        ORDER BY nm_especialidade", conn))
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


   

    protected void btnSalvar_Click(object sender, EventArgs e)
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // 1) Ler os blocos de dia da semana enviados no hidden field (JSON)
        List<BlocoDiaDTO> blocos = new List<BlocoDiaDTO>();
        if (!string.IsNullOrEmpty(hdnBlocosJson.Value))
        {
            try
            {
                blocos = serializer.Deserialize<List<BlocoDiaDTO>>(hdnBlocosJson.Value);
            }
            catch
            {
                blocos = new List<BlocoDiaDTO>();
            }
        }

        // 2) Ler os bloqueios (DE / ATÉ / MOTIVO) enviados no hidden field (JSON)
        List<BloqueioDTO> bloqueios = new List<BloqueioDTO>();
        if (!string.IsNullOrEmpty(hdnBloqueiosJson.Value))
        {
            try
            {
                bloqueios = serializer.Deserialize<List<BloqueioDTO>>(hdnBloqueiosJson.Value);
            }
            catch
            {
                bloqueios = new List<BloqueioDTO>();
            }
        }

        // 3) Montar o DTO principal (tabela PreAgendamento)
        PreAgendamentoDTO dto = new PreAgendamentoDTO();
        dto.DataPreenchimento = Convert.ToDateTime(txtDataPreenchimento.Text);
        dto.Clinica = ddlClinica.SelectedItem.Text;   // se quiser gravar o nome
        dto.Profissional = txtProfissional.Text;

  
        dto.Observacoes = txtObservacoes.Text;
        dto.Usuario = (Session["login"] ?? "desconhecido").ToString();
        dto.DataCadastro = DateTime.Now;

        try
        {
            // 4) Inserir pai + filhos (dias + bloqueios) em transação
            
            int idGerado = PreAgendamentoDAO.Inserir(dto, blocos, bloqueios);

            string scriptOk = string.Format("alert('Cadastro salvo com sucesso! ID: {0}');", idGerado);
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "ok",
                scriptOk,
                true
            );
        }
        catch (Exception ex)
        {
            string script = string.Format("alert('Erro ao salvar: {0}');", ex.Message);
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "err",
                script,
                true
            );
        }
    }
}

