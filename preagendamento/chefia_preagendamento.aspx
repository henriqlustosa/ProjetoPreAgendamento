<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="chefia_preagendamento.aspx.cs" Inherits="chefia_preagendamento"
    Title="Gerenciamento de Pré-Agendamentos (Chefia)" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        body { background-color: #f8f9fa; font-family: 'Segoe UI', sans-serif; }
        
        /* Cor diferente para indicar área de Chefia (Ex: Vinho/HSPM) */
        .card-header-chefia {
            background-color: #2A3F54; 
            color: #fff;
            font-weight: 600;
            padding: 15px 20px;
            border-radius: 8px 8px 0 0;
        }

        .card-hspm {
            border: none;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            border-radius: 8px;
        }
        
        .btn-view {
            background-color: #17a2b8;
            color: white;
            padding: 5px 10px;
            border-radius: 4px;
            text-decoration: none;
            font-size: 0.9rem;
        }
        .btn-view:hover { background-color: #138496; color: white; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">

    <div class="container py-4">
        
        <asp:Label ID="lblMensagem" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>

        <div class="card card-hspm">
            <div class="card-header card-header-chefia">
                <i class="fas fa-user-tie mr-2"></i> Painel da Chefia - Pré-Agendamentos da Minha Clínica
            </div>

            <div class="card-body">
                <!-- GridView filtrado -->
                <asp:GridView ID="gvLista" 
                              CssClass="table table-hover mb-0"
                              GridLines="None"
                              AutoGenerateColumns="false" 
                              OnRowCommand="gvLista_RowCommand"
                              runat="server">

                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="ID">
                            <ItemStyle Width="50px" Font-Bold="true" HorizontalAlign="Center" />
                            <HeaderStyle HorizontalAlign="Center" />
                        </asp:BoundField>

                        <asp:BoundField DataField="DataPreenchimento" HeaderText="Data Ref." DataFormatString="{0:dd/MM/yyyy}">
                            <ItemStyle Width="100px" />
                        </asp:BoundField>

                      
                        <asp:BoundField DataField="Clinica" HeaderText="Clínica" />
                        
                        <asp:BoundField DataField="Profissional" HeaderText="Profissional" />

                        <asp:TemplateField HeaderText="Ações">
                            <ItemStyle Width="150px" HorizontalAlign="Right" />
                            <ItemTemplate>
                                <!-- Botão de Visualizar/Detalhes (Pode redirecionar para a mesma tela de edição ou uma somente leitura) -->
                                <a href='visualizar_preagendamento.aspx?id=<%# Eval("Id") %>&mode=view' class="btn-view">
                                    <i class="fas fa-search"></i> Visualizar
                                </a>

                                <!-- Botão Excluir (Se o chefe puder excluir) -->
                                <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-danger ml-2"
                                    CommandName="Excluir" CommandArgument='<%# Eval("Id") %>'
                                    OnClientClick="return confirm('Chefia: Confirma a exclusão deste registro?');">
                                    <i class="fas fa-trash"></i>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>

                </asp:GridView>

                <div id="divEmpty" runat="server" visible="false" class="text-center py-5 text-muted">
                    <i class="fas fa-folder-open fa-3x mb-3"></i><br />
                    Não há pré-agendamentos ativos para a sua clínica.
                </div>

            </div>
        </div>
    </div>

</asp:Content>

