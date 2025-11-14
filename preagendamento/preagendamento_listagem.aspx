<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="preagendamento_listagem.aspx.cs" Inherits="publico_preagendamento_listagem"
    Title="Pré-Agendamentos - HSPM" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

    

</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">

<div class="container mt-4">

    <div class="card shadow">
        <div class="card-header bg-primary text-white">
            Pré-Agendamentos Cadastrados
        </div>

        <div class="card-body">

            <asp:GridView ID="gvLista" CssClass="table table-bordered table-striped"
                AutoGenerateColumns="false" runat="server">

                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="ID" />
                    <asp:BoundField DataField="DataPreenchimento" HeaderText="Data" DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:BoundField DataField="Clinica" HeaderText="Clínica" />
                    <asp:BoundField DataField="Profissional" HeaderText="Profissional" />

                    <asp:TemplateField HeaderText="Ações">
                        <ItemTemplate>
                            <a href='preagendamento.aspx?id=<%# Eval("Id") %>' class="btn btn-sm btn-info">Editar</a>
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-danger"
                                CommandName="Excluir" CommandArgument='<%# Eval("Id") %>'
                                Text="Excluir" />
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>

            </asp:GridView>

        </div>
    </div>

</div>

</asp:Content>


