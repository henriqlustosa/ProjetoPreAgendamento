<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="visualizar_preagendamento.aspx.cs" Inherits="visualizar_preagendamento" Title="Visualizar Pré-Agendamento" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        /* --- Estilos Gerais --- */
        body { background: #f8f9fa; font-family: 'Segoe UI', sans-serif; }
        
        /* Cards e Containers */
        .page-title-card {
            background: #2A3F54; color: #fff;
            padding: 15px 20px; border-radius: 8px 8px 0 0;
            font-weight: 600; font-size: 1.1rem;
            display: flex; align-items: center; gap: 10px;
        }
        .main-card {
            background: #fff; border-radius: 0 0 8px 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.05);
            padding: 25px; margin-bottom: 40px;
        }
        .section-card {
            border: 1px solid #e9ecef; border-radius: 8px;
            margin-bottom: 25px; overflow: hidden;
        }
        .section-header {
            background: #f8f9fa; padding: 12px 20px;
            border-bottom: 1px solid #e9ecef; border-left: 4px solid #2A3F54;
            font-weight: 600; color: #495057; display: flex; justify-content: space-between; align-items: center;
        }

        /* Formulários e Inputs (Modo Leitura) */
        .form-label { font-weight: 600; font-size: 0.85rem; color: #6c757d; margin-bottom: 4px; display: block; }
        .form-control, .form-select { 
            border-radius: 6px; font-size: 0.95rem; border: 1px solid #ced4da;
            background-color: #e9ecef; /* Cinza para indicar desabilitado */
            cursor: not-allowed;
        }
        
        /* Tabelas */
        .custom-table { width: 100%; margin-bottom: 0; font-size: 0.9rem; }
        .custom-table thead th { background-color: #fff; border-bottom: 2px solid #dee2e6; color: #495057; font-weight: 600; text-transform: uppercase; font-size: 0.75rem; padding: 12px; }
        .custom-table tbody td { vertical-align: middle; padding: 10px 12px; border-bottom: 1px solid #e9ecef; }

        /* Tags de Meses */
        .mes-tag {
            display: inline-flex; align-items: center;
            background-color: #e3f2fd; color: #0d47a1;
            padding: 6px 12px; border-radius: 20px;
            font-size: 0.85rem; font-weight: 600; margin: 4px;
            border: 1px solid #bbdefb;
        }

        /* Empty States */
        .empty-state { padding: 30px; text-align: center; color: #adb5bd; font-style: italic; }
        
        /* Botões de Ação no Rodapé */
        .footer-actions { background: #fff; padding-top: 20px; border-top: 1px solid #dee2e6; display: flex; justify-content: flex-end; gap: 10px; }

        /* --- ESTILOS ESPECÍFICOS PARA REPROVAÇÃO --- */
        .reprove-box {
            background-color: #fff5f5;
            border: 1px solid #feb2b2;
            border-radius: 8px;
            padding: 20px;
            margin-top: 20px;
            margin-bottom: 20px;
            animation: fadeIn 0.3s ease-in-out;
        }
        .reprove-title { color: #c53030; font-weight: bold; margin-bottom: 10px; display: block; }
        .reprove-input { background-color: #fff !important; cursor: text !important; border-color: #fc8181 !important; }
        
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(-10px); }
            to { opacity: 1; transform: translateY(0); }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-xl-10">

                <!-- Mensagem de Feedback -->
                <asp:Label ID="lblMensagem" runat="server" Visible="false" CssClass="d-block mb-3 p-3 rounded text-center fw-bold"></asp:Label>

                <div class="page-title-card">
                    <i class="fas fa-eye"></i>
                    <asp:Label ID="lblTituloPagina" runat="server" Text="Visualizar e Aprovar Agenda"></asp:Label>
                </div>

                <div class="main-card">
                    
                    <!-- DADOS GERAIS -->
                    <div class="section-card">
                        <div class="section-header">
                            <span><i class="fas fa-user-md me-2"></i>Dados Gerais</span>
                        </div>
                        <div class="p-4">
                            <div class="row g-3">
                                <div class="col-md-2">
                                    <span class="form-label">Data Solicitação</span>
                                    <asp:TextBox ID="txtDataPreenchimento" CssClass="form-control" ReadOnly="true" runat="server" />
                                    <asp:HiddenField ID="hdnIdPreAgendamento" runat="server" />
                                </div>
                                <div class="col-md-5">
                                    <span class="form-label">Clínica</span>
                                    <asp:DropDownList ID="ddlClinica" CssClass="form-control form-select" Enabled="false" runat="server" />
                                </div>
                                <div class="col-md-5">
                                    <span class="form-label">Profissional</span>
                                    <input type="text" id="txtProfissionalVisual" class="form-control" readonly />
                                    <asp:HiddenField ID="hdnCodProfissional" runat="server" />
                                    <asp:HiddenField ID="hdnNomeProfissional" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- MESES -->
                    <div class="section-card">
                        <div class="section-header">
                            <span><i class="far fa-calendar-alt me-2"></i>Meses da Agenda</span>
                        </div>
                        <div class="p-4">
                            <div id="containerMesesTags" class="p-3 bg-light rounded border border-light">
                                <span class="text-muted small ms-2" id="lblNenhumMes">Nenhum mês selecionado.</span>
                            </div>
                            <asp:HiddenField ID="hdnMesesSelecionados" runat="server" />
                        </div>
                    </div>

                    <!-- HORÁRIOS -->
                    <div class="section-card">
                        <div class="section-header">
                            <span><i class="far fa-clock me-2"></i>Horários Semanais</span>
                        </div>
                        
                        <div class="table-responsive">
                            <table class="table custom-table table-hover" id="tblHorarios">
                                <thead>
                                    <tr>
                                        <th style="width: 25%;">Dia</th>
                                        <th style="width: 20%;">Horário</th>
                                        <th style="width: 35%;">Subespecialidade</th>
                                        <th class="text-center" style="width: 10%;">Novas</th>
                                        <th class="text-center" style="width: 10%;">Retorno</th>
                                    </tr>
                                </thead>
                                <tbody></tbody>
                            </table>
                            <div id="emptyHorarios" class="empty-state">
                                Nenhum horário configurado.
                            </div>
                        </div>
                        <asp:HiddenField ID="hdnBlocosJson" runat="server" />
                    </div>

                    <!-- BLOQUEIOS -->
                    <div class="section-card">
                        <div class="section-header">
                            <span><i class="fas fa-ban me-2"></i>Bloqueios / Ausências</span>
                        </div>
                        
                         <div class="table-responsive">
                            <table class="table custom-table table-hover" id="tblBloqueios">
                                <thead>
                                    <tr>
                                        <th>Data Início</th>
                                        <th>Data Fim</th>
                                        <th>Motivo</th>
                                    </tr>
                                </thead>
                                <tbody></tbody>
                            </table>
                            <div id="emptyBloqueios" class="empty-state">
                                Nenhum bloqueio cadastrado.
                            </div>
                        </div>
                        <asp:HiddenField ID="hdnBloqueiosJson" runat="server" />
                    </div>

                    <!-- OBSERVAÇÕES -->
                    <div class="mb-3">
                         <span class="form-label">Observações Gerais</span>
                         <asp:TextBox ID="txtObservacoes" CssClass="form-control" TextMode="MultiLine" Rows="3" ReadOnly="true" runat="server" />
                    </div>

                    <!-- ÁREA DE REPROVAÇÃO (Visível apenas ao clicar em Reprovar) -->
                    <asp:Panel ID="pnlReprovacao" runat="server" Visible="false" CssClass="reprove-box">
                        <span class="reprove-title"><i class="fas fa-exclamation-triangle"></i> Motivo da Reprovação</span>
                        <p class="small text-muted mb-2">Por favor, descreva o motivo pelo qual este agendamento está sendo reprovado. Esta justificativa é obrigatória.</p>
                        
                        <asp:TextBox ID="txtMotivoReprovacao" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control reprove-input" placeholder="Digite aqui a justificativa..."></asp:TextBox>
                        
                        <asp:RequiredFieldValidator ID="rfvMotivo" runat="server" ControlToValidate="txtMotivoReprovacao" 
                            ValidationGroup="GrupoReprovacao" ErrorMessage="A justificativa é obrigatória." 
                            CssClass="text-danger small fw-bold mt-1 display-block"></asp:RequiredFieldValidator>

                        <div class="d-flex justify-content-end gap-2 mt-3">
                            <asp:Button ID="btnCancelarReprovacao" runat="server" Text="Cancelar" OnClick="btnCancelarReprovacao_Click" CssClass="btn btn-secondary btn-sm" CausesValidation="false" />
                            <asp:Button ID="btnConfirmarReprovacao" runat="server" Text="Confirmar Reprovação" OnClick="btnConfirmarReprovacao_Click" CssClass="btn btn-danger btn-sm fw-bold" ValidationGroup="GrupoReprovacao" />
                        </div>
                    </asp:Panel>

                    <!-- RODAPÉ DE AÇÕES -->
                    <div class="footer-actions">
                         <asp:Button ID="btnVoltar" CssClass="btn btn-light border me-auto" Text="Voltar" OnClick="btnVoltar_Click" runat="server" CausesValidation="false" />
                         
                         <!-- Botão que abre o painel de Reprovação -->
                         <asp:Button ID="btnAbrirReprovacao" runat="server" Text="✖ Reprovar Agenda" 
                            CssClass="btn btn-outline-danger px-3" 
                            OnClick="btnAbrirReprovacao_Click" CausesValidation="false" />

                         <!-- Botão de Aprovação (Escondido se estiver reprovando) -->
                         <asp:Button ID="btnAprovar" runat="server" Text="✔ Aprovar Agenda" 
                            CssClass="btn btn-success px-4" 
                            OnClick="btnAprovar_Click" 
                            OnClientClick="return confirm('Tem certeza que deseja APROVAR este pré-agendamento?');" CausesValidation="false" />
                    </div>

                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">

        // --- ESTADO DA APLICAÇÃO ---
        var listaMeses = new Map();
        var listaHorarios = [];
        var listaBloqueios = [];
        var cacheMotivos = null; // Usado para traduzir IDs se necessário

        // --- INICIALIZAÇÃO ---
        $(function () {
            // Carrega motivos primeiro para garantir tradução correta
            CarregarMotivosBackend(function () {
                CarregarDadosVisualizacao();
            });
        });

        // =========================================================
        // CARREGAR DADOS
        // =========================================================
        function CarregarDadosVisualizacao() {
            // 1. Nome do Profissional
            var nomeProf = $('#<%= hdnNomeProfissional.ClientID %>').val();
            $('#txtProfissionalVisual').val(nomeProf);

            // 2. Meses
            var strMeses = $('#<%= hdnMesesSelecionados.ClientID %>').val();
            if (strMeses) {
                strMeses.split(',').forEach(function (m) {
                    m = m.trim();
                    if (m && m.indexOf('-') > -1) {
                        var partes = m.split('-');
                        var dataObj = new Date(parseInt(partes[0]), parseInt(partes[1]) - 1, 1);
                        var label = dataObj.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
                        label = label.charAt(0).toUpperCase() + label.slice(1);
                        listaMeses.set(m, label);
                    }
                });
                RenderizarMeses();
            }

            // 3. Horarios
            var jsonH = $('#<%= hdnBlocosJson.ClientID %>').val();
            if (jsonH) {
                listaHorarios = JSON.parse(jsonH);
                RenderizarTabelaHorarios();
            }

            // 4. Bloqueios
            var jsonB = $('#<%= hdnBloqueiosJson.ClientID %>').val();
            if (jsonB) {
                listaBloqueios = JSON.parse(jsonB);

                // Tratamento de Motivo Nulo (Mesma lógica da edição)
                if (cacheMotivos && listaBloqueios.length > 0) {
                    for (var i = 0; i < listaBloqueios.length; i++) {
                        var item = listaBloqueios[i];
                        if (!item.MotivoTexto || item.MotivoTexto === 'null') {
                            for (var j = 0; j < cacheMotivos.length; j++) {
                                if (cacheMotivos[j].CodMotivo == item.CodMotivo) {
                                    item.MotivoTexto = cacheMotivos[j].NmMotivo;
                                    break;
                                }
                            }
                        }
                        if (!item.MotivoTexto) item.MotivoTexto = "-";
                    }
                }
                RenderizarTabelaBloqueios();
            }
        }

        // =========================================================
        // RENDERING
        // =========================================================

        function RenderizarMeses() {
            var container = $('#containerMesesTags');
            container.empty();

            if (listaMeses.size === 0) {
                container.append('<span class="text-muted small ms-2" id="lblNenhumMes">Nenhum mês selecionado.</span>');
                return;
            }

            listaMeses.forEach(function (label, chave) {
                // Tag simples sem botão de remover
                var tag = $('<div class="mes-tag">' + label + '</div>');
                container.append(tag);
            });
        }

        function RenderizarTabelaHorarios() {
            var tbody = $('#tblHorarios tbody');
            tbody.empty();

            if (listaHorarios.length === 0) {
                $('#emptyHorarios').show();
                $('#tblHorarios').hide();
                return;
            }

            $('#emptyHorarios').hide();
            $('#tblHorarios').show();

            listaHorarios.forEach(function (item) {
                var tr = $('<tr>');
                tr.append('<td><strong>' + item.DiaSemana + '</strong></td>');
                tr.append('<td>' + item.Horario + '</td>');

                // Exibe texto ou traço se vazio
                var sub = item.SubespecialidadeTexto || "-";
                tr.append('<td>' + sub + '</td>');

                var badgeNovas = item.ConsultasNovas > 0 ? '<span class="badge bg-primary rounded-pill">' + item.ConsultasNovas + '</span>' : '-';
                var badgeRetorno = item.ConsultasRetorno > 0 ? '<span class="badge bg-info text-dark rounded-pill">' + item.ConsultasRetorno + '</span>' : '-';

                tr.append('<td class="text-center">' + badgeNovas + '</td>');
                tr.append('<td class="text-center">' + badgeRetorno + '</td>');
                // Sem coluna de ações

                tbody.append(tr);
            });
        }

        function RenderizarTabelaBloqueios() {
            var tbody = $('#tblBloqueios tbody');
            tbody.empty();

            if (listaBloqueios.length === 0) {
                $('#emptyBloqueios').show();
                $('#tblBloqueios').hide();
                return;
            }

            $('#emptyBloqueios').hide();
            $('#tblBloqueios').show();

            listaBloqueios.forEach(function (item) {
                var fmtData = function (d) { return d.split('-').reverse().join('/'); };

                var tr = $('<tr>');
                tr.append('<td>' + fmtData(item.De) + '</td>');
                tr.append('<td>' + fmtData(item.Ate) + '</td>');
                tr.append('<td>' + item.MotivoTexto + '</td>');
                // Sem coluna de ações

                tbody.append(tr);
            });
        }

        function CarregarMotivosBackend(callback) {
            PageMethods.ListarMotivosBloqueio(function (res) {
                cacheMotivos = res;
                if (callback) callback();
            });
        }

    </script>
</asp:Content>