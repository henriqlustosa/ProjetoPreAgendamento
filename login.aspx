﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<!DOCTYPE html>
<html>
<head>
    <title>Pré-Agendamento</title>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;600&display=swap" rel="stylesheet">
    <link href="css/login_responsivo.css" rel="stylesheet" />
 
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <script>
        $(document).ready(function () {
            if (!localStorage.getItem("modalAvisoExibido")) {
                $('#modalAviso').addClass('active').hide().fadeIn();
                localStorage.setItem("modalAvisoExibido", "true");
                sessionStorage.setItem("modalAvisoSessao", "true");
            }

            $('#btnFecharModal').click(function () {
                $('#modalAviso').fadeOut(function () {
                    $(this).removeClass('active');
                });
            });

            window.addEventListener("beforeunload", function () {
                sessionStorage.removeItem("modalAvisoSessao");
                setTimeout(function () {
                    if (performance.navigation.type !== performance.navigation.TYPE_RELOAD) {
                        localStorage.removeItem("modalAvisoExibido");
                    }
                }, 1000);
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login__container">
            <div class="login__titulo">Pré-Agendamento - Agendas Médicas</div>

            <div class="login__imagem">
                <img src="img/ImagemLogin.jpg" alt="Arquivo SAC" />
            </div>

            <div class="login__box">
                <div class="login__box-autentica">
                    <h2>Login*</h2>
                    <asp:Label ID="lblUsuario" runat="server" Text="Usuário:" AssociatedControlID="txtUsuario"></asp:Label>
                    <asp:TextBox ID="txtUsuario" runat="server"></asp:TextBox>

                    <asp:Label ID="lblSenha" runat="server" Text="Senha:" AssociatedControlID="txtSenha"></asp:Label>
                    <asp:TextBox ID="txtSenha" runat="server" TextMode="Password"></asp:TextBox>

                    <div class="login__box-botao">
                        <asp:Button ID="btnLogin" runat="server" Text="Entrar" OnClick="btnLogin_Click" />
                    </div>

                    <asp:Label ID="lblMensagem" runat="server" ForeColor="Red"></asp:Label>

                    <div class="login__logo">
                        <img class="logo" src="img/logoHspmPrefeituraColor.jpg" />
                    </div>
                </div>

                <div class="login__box-informacao">
                    <p class="login__box-informacao-texto">* Usar o mesmo login e senha de rede.</p>
                </div>
            </div>
        </div>
    </form>

    <footer>
        <p>Desenvolvido por DITEC (Divisão de Tecnologia da Informação) - hspminformatica@hspm.sp.gov.br</p>
    </footer>

    <!-- Modal de Aviso -->
    <div id="modalAviso" class="modal-overlay" role="dialog" aria-labelledby="modalTitle">
        <div class="modal-content">
            <h2 id="modalTitle">Aviso de Acesso</h2>
            <p>Utilizar as credenciais de rede (usuário e senha que você utiliza para acessar o computador).</p>

            <div class="exemplo-box">
                <strong>Exemplo:</strong><br />
                Se você acessa o computador com:<br />
                Usuário: <code>H123567</code><br />
                Senha: <code>Ab12345678</code><br />
                Utilizará essas mesmas credenciais para acessar o sistema.
            </div>

            <p>Em caso de dúvidas ou dificuldades de acesso:<br />
                Ramal <strong>8123 / 8124 / 8169 / 3310 / 8125</strong>
            </p>

            <button id="btnFecharModal">Fechar</button>
        </div>
    </div>
</body>
</html>
