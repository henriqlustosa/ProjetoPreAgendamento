<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="cadastro_profissional.aspx.cs" Inherits="cadastro_profissional" Title="Gestão de Profissionais" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    <script src="https://cdn.tailwindcss.com"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />

    <div class="min-h-screen bg-gray-50 p-6 font-sans text-gray-800">
        
        <asp:Label ID="lblMensagem" runat="server" Visible="false" CssClass="block mb-4 p-4 rounded-lg text-center font-bold shadow-sm"></asp:Label>

        <div class="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-3 gap-8">
            
            <!-- COLUNA 1: FORMULÁRIO (Mantido igual) -->
            <div class="lg:col-span-1">
                <div class="bg-white rounded-xl shadow-md overflow-hidden border border-gray-100 sticky top-6">
                    <div class="bg-blue-600 px-6 py-4 border-b border-blue-700">
                        <h2 class="text-white font-bold text-lg flex items-center gap-2">
                            <i class="fas fa-user-md"></i> Novo Profissional
                        </h2>
                    </div>
                    
                    <div class="p-6 space-y-4">
                        <div>
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Nome Completo</label>
                            <asp:TextBox ID="txtNome" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition" placeholder="Ex: Dr. João Silva"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvNome" runat="server" ControlToValidate="txtNome" ErrorMessage="Nome é obrigatório" CssClass="text-xs text-red-500 font-bold" ValidationGroup="Cadastro"></asp:RequiredFieldValidator>
                        </div>

                        <div>
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Especialidade Principal</label>
                            <div class="relative">
                                <asp:DropDownList ID="ddlEspecialidade" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg appearance-none focus:ring-2 focus:ring-blue-500 outline-none bg-white">
                                </asp:DropDownList>
                                <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-gray-700">
                                    <i class="fas fa-chevron-down text-xs"></i>
                                </div>
                            </div>
                            <asp:RequiredFieldValidator ID="rfvEsp" runat="server" ControlToValidate="ddlEspecialidade" InitialValue="0" ErrorMessage="Selecione uma especialidade" CssClass="text-xs text-red-500 font-bold" ValidationGroup="Cadastro"></asp:RequiredFieldValidator>
                        </div>

                        <div class="grid grid-cols-2 gap-4">
                            <div>
                                <label class="block text-sm font-semibold text-gray-700 mb-1">Andar / Sala</label>
                                <asp:TextBox ID="txtAndar" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="Ex: 3º Andar"></asp:TextBox>
                            </div>
                            <div class="flex items-center pt-6">
                                <label class="flex items-center space-x-2 cursor-pointer">
                                    <asp:CheckBox ID="chkChefe" runat="server" />
                                    <span class="text-sm font-medium text-gray-700">É Chefe?</span>
                                </label>
                            </div>
                        </div>

                        <div class="bg-gray-50 p-3 rounded-lg border border-gray-200">
                            <label class="flex items-center space-x-2 cursor-pointer">
                                <asp:CheckBox ID="chkAtivo" runat="server" Checked="true" />
                                <span class="text-sm font-medium text-gray-700">Cadastro Ativo</span>
                            </label>
                        </div>

                        <div class="pt-2">
                            <asp:Button ID="btnSalvar" runat="server" Text="Cadastrar Profissional" OnClick="btnSalvar_Click" ValidationGroup="Cadastro"
                                CssClass="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 rounded-lg shadow transition duration-200 cursor-pointer" />
                        </div>
                    </div>
                </div>
            </div>

            <!-- COLUNA 2: LISTAGEM -->
            <div class="lg:col-span-2">
                <div class="bg-white rounded-xl shadow-md overflow-hidden border border-gray-100 flex flex-col h-full">
                    
                    <!-- Barra de Topo da Lista -->
                    <div class="px-6 py-4 border-b border-gray-200 flex flex-col sm:flex-row justify-between items-center gap-4 bg-gray-50">
                        <h3 class="font-bold text-gray-700 text-lg">Profissionais Cadastrados</h3>
                        
                        <!-- Busca Rápida (SERVER SIDE) -->
                        <asp:Panel ID="pnlBusca" runat="server" DefaultButton="btnBuscar" CssClass="relative w-full sm:w-64">
                            <asp:LinkButton ID="btnBuscar" runat="server" OnClick="btnBuscar_Click" 
                                CssClass="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-400 hover:text-blue-500 transition">
                                <i class="fas fa-search"></i>
                            </asp:LinkButton>
                            
                            <asp:TextBox ID="txtBusca" runat="server" 
                                placeholder="Buscar por nome..." 
                                OnTextChanged="btnBuscar_Click" AutoPostBack="true"
                                CssClass="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"></asp:TextBox>
                        </asp:Panel>
                    </div>

                    <!-- Tabela -->
                    <div class="overflow-x-auto flex-grow">
                        <table class="w-full text-left border-collapse">
                            <thead>
                                <tr class="bg-gray-100 text-gray-600 text-xs uppercase tracking-wider border-b border-gray-200">
                                    <th class="p-4 font-semibold">Nome</th>
                                    <th class="p-4 font-semibold">Especialidade</th>
                                    <th class="p-4 font-semibold">Detalhes</th>
                                    <th class="p-4 font-semibold text-center">Status</th>
                                    <th class="p-4 font-semibold text-center">Ações</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-100">
                                <asp:Repeater ID="rptProfissionais" runat="server" OnItemCommand="rptProfissionais_ItemCommand">
                                    <ItemTemplate>
                                        <tr class="hover:bg-blue-50 transition-colors duration-150">
                                            <td class="p-4 font-medium text-gray-900">
                                                <%# Eval("nome_profissional") %>
                                                <div class="text-xs text-gray-400 font-mono">ID: <%# Eval("cod_profissional") %></div>
                                            </td>
                                            <td class="p-4 text-gray-600">
                                                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                                                    <%# Eval("nm_especialidade") %>
                                                </span>
                                            </td>
                                            <td class="p-4 text-sm text-gray-500">
                                                <%# Convert.ToBoolean(Eval("chefe")) ? "<span class='text-yellow-600 font-bold'><i class='fas fa-crown mr-1'></i>Chefe</span>" : "" %>
                                                <%# !string.IsNullOrEmpty(Eval("andar").ToString()) ? "<div class='mt-1'><i class='far fa-building mr-1'></i>" + Eval("andar") + "</div>" : "" %>
                                            </td>
                                            <td class="p-4 text-center">
                                                <%# Convert.ToBoolean(Eval("ativo")) 
                                                    ? "<span class='inline-block w-3 h-3 bg-green-500 rounded-full' title='Ativo'></span>" 
                                                    : "<span class='inline-block w-3 h-3 bg-red-500 rounded-full' title='Inativo'></span>" %>
                                            </td>
                                            <td class="p-4 text-center">
                                                <asp:LinkButton ID="btnExcluir" runat="server" CommandName="Excluir" CommandArgument='<%# Eval("cod_profissional") %>'
                                                    CssClass="text-red-500 hover:text-red-700 transition" OnClientClick="return confirm('Tem certeza que deseja desativar este profissional?');">
                                                    <i class="far fa-trash-alt"></i>
                                                </asp:LinkButton>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                        
                        <!-- Estado Vazio -->
                        <div id="divEmpty" runat="server" visible="false" class="p-8 text-center text-gray-500">
                            <i class="far fa-folder-open text-4xl mb-3 text-gray-300"></i>
                            <p>Nenhum profissional encontrado.</p>
                        </div>
                    </div>

                    <!-- PAGINAÇÃO -->
                    <div class="px-6 py-4 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
                        <span class="text-sm text-gray-500">
                            Página <asp:Label ID="lblPageInfo" runat="server" Text="1" Font-Bold="true"></asp:Label>
                        </span>
                        <div class="flex space-x-2">
                            <asp:LinkButton ID="btnAnt" runat="server" OnClick="btnAnt_Click" 
                                CssClass="px-3 py-1 bg-white border border-gray-300 rounded text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                <i class="fas fa-chevron-left mr-1"></i> Anterior
                            </asp:LinkButton>
                            
                            <asp:LinkButton ID="btnProx" runat="server" OnClick="btnProx_Click" 
                                CssClass="px-3 py-1 bg-white border border-gray-300 rounded text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                Próximo <i class="fas fa-chevron-right ml-1"></i>
                            </asp:LinkButton>
                        </div>
                    </div>

                </div>
            </div>
        </div>
    </div>
</asp:Content>