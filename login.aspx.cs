using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Web.Security;

public partial class Login : System.Web.UI.Page
{

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        string login = txtUsuario.Text.Trim();
        string senha = txtSenha.Text.Trim();

        try
        {
            // 1. Autenticação no AD
            using (DirectoryEntry entry = new DirectoryEntry("LDAP://10.10.68.43", login, senha))
            {
                object nativeObject = entry.NativeObject; // Apenas tenta autenticar

                // 🔍 Obter nome completo do usuário
                string nomeCompleto = "";
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = "(sAMAccountName=" + login + ")";
                    searcher.PropertiesToLoad.Add("displayName");

                    SearchResult result = searcher.FindOne();
                    if (result != null && result.Properties.Contains("displayname"))
                    {
                        nomeCompleto = result.Properties["displayname"][0].ToString();
                    }
                }


                // 2. Verificar se o usuário está cadastrado no sistema
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["gtaConnectionString"].ToString()))
                {
                    string sql = "SELECT Id FROM Usuarios WHERE LoginRede = @LoginRede";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@LoginRede", login);

                    con.Open();
                    object usuarioIdObj = cmd.ExecuteScalar();
                    con.Close();

                    if (usuarioIdObj == null)
                    {
                        lblMensagem.Text = "Usuário sem permissão de acesso.";
                        return;
                    }

                    int usuarioId = Convert.ToInt32(usuarioIdObj);
                    List<int> perfisDoUsuario = new List<int>();

                    // 3. Buscar os perfis do usuário
                    using (SqlConnection con2 = new SqlConnection(ConfigurationManager.ConnectionStrings["gtaConnectionString"].ToString()))
                    {
                        string query = @"
                    SELECT up.PerfilId
                    FROM UsuariosPerfis up
                    WHERE up.UsuarioId = @UsuarioId";

                        SqlCommand cmd2 = new SqlCommand(query, con2);
                        cmd2.Parameters.AddWithValue("@UsuarioId", usuarioId);
                        con2.Open();
                        SqlDataReader reader = cmd2.ExecuteReader();

                        while (reader.Read())
                        {
                            perfisDoUsuario.Add(Convert.ToInt32(reader["PerfilId"]));
                        }
                        reader.Close();
                    }

                    if (perfisDoUsuario.Count == 0)
                    {
                        lblMensagem.Text = "Usuário sem perfil de acesso.";
                        return;
                    }

                    // 4. Salvar informações na sessão
                    Session["login"] = login;
                    Session["perfis"] = perfisDoUsuario;
                    Session["nomeUsuario"] = nomeCompleto; // nome do usuário

                    // 5. Redirecionar
                    Response.Redirect("~/Encaminhamento/cadencaminhamento.aspx");
                }
            }
        }
        catch (Exception ex)
        {
            lblMensagem.Text = "Login inválido. Verifique seu usuário e senha.";
        }
    }
}
