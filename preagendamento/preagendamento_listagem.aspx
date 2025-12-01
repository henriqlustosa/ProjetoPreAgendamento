<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="preagendamento_listagem.aspx.cs" Inherits="publico_preagendamento_listagem"
    Title="Pré-Agendamentos - HSPM" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        /* Fundo geral igual ao da Master */
        body {
            background-color: #f8f9fa;
            font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
        }

        /* Estilo do Card principal */
        .card-hspm {
            border: none;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }

        /* Cabeçalho com a cor do Menu Lateral (#2A3F54) */
        .card-header-hspm {
            background-color: #2A3F54; /* Cor exata do menu lateral */
            color: #fff;
            font-weight: 600;
            font-size: 1.1rem;
            padding: 15px 20px;
            border-radius: 8px 8px 0 0 !important;
        }

        /* Ajuste da tabela */
        .table-custom thead th {
            background-color: #f1f3f5;
            color: #2A3F54;
            border-bottom: 2px solid #dee2e6;
            font-weight: 600;
        }

        /* Botão Editar (Azul escuro do tema) */
        .btn-hspm-edit {
            background-color: #2A3F54;
            border-color: #2A3F54;
            color: #fff;
            padding: 4px 10px;
            font-size: 0.85rem;
            border-radius: 4px;
            text-decoration: none;
            transition: all 0.2s;
        }

            .btn-hspm-edit:hover {
                background-color: #1f2f40;
                color: #fff;
            }

        /* Botão Excluir (Vermelho suave ou outline) */
        .btn-hspm-delete {
            background-color: #fff;
            border: 1px solid #dc3545;
            color: #dc3545;
            padding: 4px 10px;
            font-size: 0.85rem;
            border-radius: 4px;
            text-decoration: none;
            transition: all 0.2s;
        }

            .btn-hspm-delete:hover {
                background-color: #dc3545;
                color: #fff;
            }

        /* Espaçamento do container */
        .listagem-container {
            padding: 30px 0;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">

    <div class="container listagem-container">

        <div class="card card-hspm">
            <div class="card-header card-header-hspm">
                <i class="fas fa-list-ul mr-2"></i>Pré-Agendamentos Cadastrados
            </div>

            <div class="card-body">
                <asp:GridView ID="gvLista"
                    CssClass="table table-hover table-custom mb-0"
                    GridLines="None"
                    AutoGenerateColumns="false"
                    OnRowCommand="gvLista_RowCommand"
                    runat="server">

                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="ID">
                            <ItemStyle Width="60px" Font-Bold="true" HorizontalAlign="Center" />
                            <HeaderStyle HorizontalAlign="Center" />
                        </asp:BoundField>

                        <asp:BoundField DataField="DataPreenchimento" HeaderText="Data" DataFormatString="{0:dd/MM/yyyy}">
                            <ItemStyle Width="120px" />
                        </asp:BoundField>

                        <asp:BoundField DataField="Clinica" HeaderText="Clínica" />

                        <asp:BoundField DataField="Profissional" HeaderText="Profissional" />

                        <asp:TemplateField HeaderText="Ações">
                            <ItemStyle Width="180px" HorizontalAlign="Center" />
                            <HeaderStyle HorizontalAlign="Center" />
                            <ItemTemplate>
                                <a href='preagendamento.aspx?id=<%# Eval("Id") %>' class="btn-hspm-edit">
                                    <i class="fas fa-edit"></i>Editar
                                </a>

                                <asp:LinkButton runat="server" CssClass="btn-hspm-delete ml-2"
                                    CommandName="Excluir" CommandArgument='<%# Eval("Id") %>'
                                    OnClientClick="return confirm('Tem certeza que deseja excluir este registro?');">
                                    <i class="fas fa-trash-alt"></i> Excluir
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>

                </asp:GridView>

                <div id="divEmpty" runat="server" visible="false" class="text-center py-4 text-muted">
                    Nenhum registro encontrado.
                </div>

            </div>
        </div>

    </div>

</asp:Content>
