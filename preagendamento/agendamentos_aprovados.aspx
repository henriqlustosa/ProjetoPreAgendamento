<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="agendamentos_aprovados.aspx.cs" Inherits="agendamentos_aprovados" Title="Histórico de Agendamentos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    <script src="https://cdn.tailwindcss.com"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    
    <div class="min-h-screen bg-gray-50 p-4 font-sans text-gray-800">
        
        <asp:Label ID="lblMensagem" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>

        <!-- Header -->
        <div class="max-w-6xl mx-auto mb-8">
            <div class="bg-white rounded-lg shadow-sm p-6 border-l-4 border-blue-500 flex flex-col md:flex-row justify-between items-center gap-4">
                <div>
                    <h1 class="text-2xl font-bold text-gray-800 flex items-center gap-2">
                        <i class="fas fa-history text-blue-500"></i>
                        Histórico de Agendamentos
                    </h1>
                    <p class="text-gray-500 mt-1">
                        Visualizando registros finalizados (<strong>Aprovados</strong> e <strong>Reprovados</strong>)
                    </p>
                </div>
                
                <!-- Campo de Busca -->
                <div class="relative w-full md:w-64">
                    <span class="absolute inset-y-0 left-0 flex items-center pl-3">
                        <i class="fas fa-search text-gray-400"></i>
                    </span>
                    <input type="text" id="searchInput" onkeyup="filtrarTabela()" 
                           placeholder="Buscar profissional..." 
                           class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all">
                </div>
            </div>
        </div>

        <!-- Tabela -->
        <asp:Panel ID="pnlTabela" runat="server" CssClass="max-w-6xl mx-auto bg-white rounded-xl shadow-md overflow-hidden">
            <div class="overflow-x-auto">
                <table class="w-full text-left border-collapse" id="tabelaAgendamentos">
                    <thead>
                        <tr class="bg-gray-100 border-b border-gray-200 text-gray-600 text-sm uppercase tracking-wider">
                            <th class="p-4 font-semibold">ID</th>
                            <th class="p-4 font-semibold">Profissional</th>
                            <th class="p-4 font-semibold">Clínica</th>
                            <th class="p-4 font-semibold">Data Ref.</th>
                            <th class="p-4 font-semibold text-center">Status</th>
                            <th class="p-4 font-semibold text-center">Ações</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                        <asp:Repeater ID="rptAgendamentos" runat="server" OnItemCommand="rptAgendamentos_ItemCommand">
                            <ItemTemplate>
                                <tr class="hover:bg-gray-50 transition-colors duration-150 item-row">
                                    <td class="p-4 text-gray-500 font-mono">
                                        #<%# Eval("id") %>
                                    </td>
                                    <td class="p-4 font-medium text-gray-900 nome-cliente">
                                        <%# Eval("Profissional") %> 
                                    </td>
                                    <td class="p-4 text-gray-600">
                                        <%# Eval("Clinica") %>
                                    </td>
                                    <td class="p-4 text-gray-600">
                                        <div class="flex items-center gap-1">
                                            <i class="far fa-calendar-alt text-gray-400"></i> 
                                            <%# Eval("data_preenchimento", "{0:dd/MM/yyyy}") %>
                                        </div>
                                    </td>
                                    <td class="p-4 text-center">
                                        <!-- Badge Dinâmico (Verde ou Vermelho) -->
                                        <span class='<%# ObterClasseStatus(Eval("status")) %>'>
                                            <%# ObterTextoStatus(Eval("status")) %>
                                        </span>
                                    </td>
                                    <td class="p-4 text-center">
                                        <asp:LinkButton ID="btnVisualizar" runat="server"
                                                        CommandName="Visualizar"
                                                        CommandArgument='<%# Eval("id") %>'
                                                        CssClass="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-all shadow-sm hover:shadow-md border-none cursor-pointer">
                                            <i class="fas fa-eye"></i> Visualizar
                                        </asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
            
            <div class="bg-gray-50 px-6 py-4 border-t border-gray-200 text-sm text-gray-500 flex justify-between">
                <span>Lista atualizada do sistema</span>
            </div>
        </asp:Panel>

        <!-- Estado Vazio -->
        <div id="divEmpty" runat="server" visible="false" class="max-w-6xl mx-auto mt-8 text-center p-8 bg-white rounded-lg border border-dashed border-gray-300">
            <i class="far fa-folder-open text-gray-400 text-4xl mb-3"></i>
            <p class="text-gray-500 text-lg">Nenhum registro encontrado.</p>
        </div>

    </div>

    <script>
        function filtrarTabela() {
            var input, filter, table, tr, td, i, txtValue;
            input = document.getElementById("searchInput");
            filter = input.value.toUpperCase();
            table = document.getElementById("tabelaAgendamentos");
            tr = table.getElementsByTagName("tr");

            for (i = 0; i < tr.length; i++) {
                td = tr[i].getElementsByClassName("nome-cliente")[0];
                if (td) {
                    txtValue = td.textContent || td.innerText;
                    if (txtValue.toUpperCase().indexOf(filter) > -1) {
                        tr[i].style.display = "";
                    } else {
                        tr[i].style.display = "none";
                    }
                }
            }
        }
    </script>

</asp:Content>