<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="preagendamento.aspx.cs" Inherits="publico_preagendamento"
    Title="Pré-Agendamento - Cadastro" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">

    <style>
        /* --- Estilos Gerais --- */
        body {
            background: #f8f9fa;
            font-family: 'Segoe UI', sans-serif;
        }

        /* Cards e Containers */
        .page-title-card {
            background: #2A3F54;
            color: #fff;
            padding: 15px 20px;
            border-radius: 8px 8px 0 0;
            font-weight: 600;
            font-size: 1.1rem;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .main-card {
            background: #fff;
            border-radius: 0 0 8px 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.05);
            padding: 25px;
            margin-bottom: 40px;
        }

        .section-card {
            border: 1px solid #e9ecef;
            border-radius: 8px;
            margin-bottom: 25px;
            overflow: hidden;
        }

        .section-header {
            background: #f8f9fa;
            padding: 12px 20px;
            border-bottom: 1px solid #e9ecef;
            border-left: 4px solid #2A3F54;
            font-weight: 600;
            color: #495057;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        /* Formulários e Inputs */
        .form-label {
            font-weight: 600;
            font-size: 0.85rem;
            color: #6c757d;
            margin-bottom: 4px;
            display: block;
        }

        .form-control, .form-select {
            border-radius: 6px;
            font-size: 0.95rem;
            border: 1px solid #ced4da;
        }

            .form-control:focus {
                border-color: #2A3F54;
                box-shadow: 0 0 0 0.2rem rgba(42, 63, 84, 0.15);
            }

        .required-asterisk {
            color: #dc3545;
            margin-left: 3px;
        }

        /* Área de Adição (Fundo Cinza) */
        .add-area {
            background-color: #f1f3f5;
            padding: 20px;
            border-bottom: 1px solid #dee2e6;
        }

        /* Tabelas */
        .custom-table {
            width: 100%;
            margin-bottom: 0;
            font-size: 0.9rem;
        }

            .custom-table thead th {
                background-color: #fff;
                border-bottom: 2px solid #dee2e6;
                color: #495057;
                font-weight: 600;
                text-transform: uppercase;
                font-size: 0.75rem;
                padding: 12px;
            }

            .custom-table tbody td {
                vertical-align: middle;
                padding: 10px 12px;
                border-bottom: 1px solid #e9ecef;
            }

        .btn-action-icon {
            color: #dc3545;
            cursor: pointer;
            background: none;
            border: none;
            padding: 5px;
        }

            .btn-action-icon:hover {
                color: #a71d2a;
                background-color: #ffe3e3;
                border-radius: 4px;
            }

        /* Tags de Meses */
        .mes-tag {
            display: inline-flex;
            align-items: center;
            background-color: #e3f2fd;
            color: #0d47a1;
            padding: 6px 12px;
            border-radius: 20px;
            font-size: 0.85rem;
            font-weight: 600;
            margin: 4px;
            border: 1px solid #bbdefb;
        }

            .mes-tag .btn-remove {
                margin-left: 8px;
                cursor: pointer;
                color: #1565c0;
                font-size: 0.9rem;
            }

                .mes-tag .btn-remove:hover {
                    color: #d32f2f;
                }

        /* Empty States */
        .empty-state {
            padding: 30px;
            text-align: center;
            color: #adb5bd;
            font-style: italic;
        }

        /* Botões */
        .btn-add-custom {
            background-color: #2A3F54;
            color: white;
            border: none;
            padding: 8px 20px;
            border-radius: 6px;
            font-size: 0.9rem;
            transition: all 0.2s;
        }

            .btn-add-custom:hover {
                background-color: #1a2633;
                transform: translateY(-1px);
            }

        .footer-actions {
            background: #fff;
            padding-top: 20px;
            border-top: 1px solid #dee2e6;
            display: flex;
            justify-content: flex-end;
            gap: 10px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-xl-10">

                <div class="page-title-card">
                    <i class="fas fa-calendar-check"></i>
                    <asp:Label ID="lblTituloPagina" runat="server" Text="Cadastro de Agenda Médica"></asp:Label>
                </div>

                <div class="main-card">

                    <div class="section-card">
                        <div class="section-header">
                            <span><i class="fas fa-user-md me-2"></i>Dados Gerais</span>
                        </div>
                        <div class="p-4">
                            <div class="row g-3">
                                <div class="col-md-2">
                                    <span class="form-label">Data</span>
                                    <asp:TextBox ID="txtDataPreenchimento" CssClass="form-control bg-light" ReadOnly="true" runat="server" />
                                    <asp:HiddenField ID="hdnIdPreAgendamento" runat="server" />
                                </div>
                                <div class="col-md-5">
                                    <span class="form-label">Clínica <span class="required-asterisk">*</span></span>
                                    <asp:DropDownList ID="ddlClinica" CssClass="form-control form-select" runat="server" />
                                </div>
                                <div class="col-md-5">
                                    <span class="form-label">Profissional <span class="required-asterisk">*</span></span>
                                    <select id="selProfissional" class="form-control form-select">
                                        <option value="">Selecione...</option>
                                    </select>
                                    <asp:HiddenField ID="hdnCodProfissional" runat="server" />
                                    <asp:HiddenField ID="hdnNomeProfissional" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="section-card">
                        <div class="section-header">
                            <span><i class="far fa-calendar-alt me-2"></i>Meses da Agenda</span>
                        </div>
                        <div class="p-4">
                            <div class="row align-items-end g-3 mb-3">
                                <div class="col-md-5">
                                    <span class="form-label">Selecionar mês para adicionar <span class="required-asterisk">*</span></span>
                                    <select id="ddlMesesDisponiveis" class="form-control form-select"></select>
                                </div>
                                <div class="col-md-3">
                                    <button type="button" id="btnAdicionarMes" class="btn btn-outline-primary w-100">
                                        <i class="fas fa-plus me-1"></i>Adicionar Mês
                                    </button>
                                </div>
                            </div>
                            <div id="containerMesesTags" class="p-3 bg-light rounded border border-light">
                                <span class="text-muted small ms-2" id="lblNenhumMes">Nenhum mês selecionado.</span>
                            </div>
                            <asp:HiddenField ID="hdnMesesSelecionados" runat="server" />
                        </div>
                    </div>

                    <div class="section-card">
                        <div class="section-header">
                            <span><i class="far fa-clock me-2"></i>Horários Semanais</span>
                        </div>

                        <div class="add-area">
                            <div class="row g-3 align-items-end">
                                <div class="col-md-3">
                                    <span class="form-label">Dia da Semana <span class="required-asterisk">*</span></span>
                                    <select id="newDia" class="form-control form-select">
                                        <option value="">Selecione...</option>
                                        <option value="Segunda">Segunda-feira</option>
                                        <option value="Terça">Terça-feira</option>
                                        <option value="Quarta">Quarta-feira</option>
                                        <option value="Quinta">Quinta-feira</option>
                                        <option value="Sexta">Sexta-feira</option>
                                        <option value="Sábado">Sábado</option>
                                        <option value="Domingo">Domingo</option>
                                    </select>
                                </div>
                                <div class="col-md-2">
                                    <span class="form-label">Horário <span class="required-asterisk">*</span></span>
                                    <input type="time" id="newHora" class="form-control" />
                                </div>
                                <div class="col-md-2">
                                    <span class="form-label">Novas</span>
                                    <input type="number" id="newNovas" class="form-control" placeholder="0" min="0" />
                                </div>
                                <div class="col-md-2">
                                    <span class="form-label">Retorno</span>
                                    <input type="number" id="newRetorno" class="form-control" placeholder="0" min="0" />
                                </div>
                                <div class="col-md-3">
                                    <button type="button" id="btnAddItemHorario" class="btn btn-add-custom w-100">
                                        <i class="fas fa-plus-circle me-1"></i>Adicionar
                                    </button>
                                </div>
                            </div>
                            <div class="row mt-3">
                                <div class="col-md-6">
                                    <span class="form-label">Subespecialidade <span class="required-asterisk">*</span></span>
                                    <select id="newSub" class="form-control form-select">
                                        <option value="">Selecione...</option>
                                    </select>
                                </div>
                            </div>
                        </div>

                        <div class="table-responsive">
                            <table class="table custom-table table-hover" id="tblHorarios">
                                <thead>
                                    <tr>
                                        <th style="width: 20%;">Dia</th>
                                        <th style="width: 15%;">Horário</th>
                                        <th style="width: 25%;">Subespecialidade</th>
                                        <th class="text-center" style="width: 15%;">Novas</th>
                                        <th class="text-center" style="width: 15%;">Retorno</th>
                                        <th class="text-end" style="width: 10%;">Ações</th>
                                    </tr>
                                </thead>
                                <tbody></tbody>
                            </table>
                            <div id="emptyHorarios" class="empty-state">
                                Nenhum horário configurado. Utilize os campos acima para adicionar.
                            </div>
                        </div>
                        <asp:HiddenField ID="hdnBlocosJson" runat="server" />
                    </div>

                    <div class="section-card">
                        <div class="section-header">
                            <span><i class="fas fa-ban me-2"></i>Bloqueios / Ausências</span>
                        </div>

                        <div class="add-area">
                            <div class="row g-3 align-items-end">
                                <div class="col-md-3">
                                    <span class="form-label">De</span>
                                    <input type="date" id="newBloqDe" class="form-control" />
                                </div>
                                <div class="col-md-3">
                                    <span class="form-label">Até</span>
                                    <input type="date" id="newBloqAte" class="form-control" />
                                </div>
                                <div class="col-md-4">
                                    <span class="form-label">Motivo</span>
                                    <select id="newBloqMotivo" class="form-control form-select">
                                        <option value="">Selecione...</option>
                                    </select>
                                </div>
                                <div class="col-md-2">
                                    <button type="button" id="btnAddItemBloqueio" class="btn btn-outline-danger w-100">
                                        <i class="fas fa-lock me-1"></i>Bloquear
                                    </button>
                                </div>
                            </div>
                        </div>

                        <div class="table-responsive">
                            <table class="table custom-table table-hover" id="tblBloqueios">
                                <thead>
                                    <tr>
                                        <th>Data Início</th>
                                        <th>Data Fim</th>
                                        <th>Motivo</th>
                                        <th class="text-end">Ações</th>
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

                    <div class="mb-3">
                        <span class="form-label">Observações Gerais</span>
                        <asp:TextBox ID="txtObservacoes" CssClass="form-control" TextMode="MultiLine" Rows="3" runat="server" />
                    </div>

                    <div class="footer-actions">
                        <asp:Button ID="btnLimpar" CssClass="btn btn-light border" Text="Limpar Formulário" OnClick="btnLimpar_Click" runat="server" />
                        <asp:Button ID="btnSalvar" CssClass="btn btn-primary px-4" Text="Salvar Agenda" OnClick="btnSalvar_Click" OnClientClick="return ValidarAntesDeSalvar();" runat="server" />
                    </div>

                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="errosModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title"><i class="fas fa-exclamation-circle me-2"></i>Atenção</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <ul id="listaErros" class="mb-0 text-danger fw-bold"></ul>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Fechar</button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">

        // --- ESTADO DA APLICAÇÃO ---
        var listaMeses = new Map(); // Chave: '2025-01', Valor: 'Janeiro 2025'
        var listaHorarios = [];     // Array de objetos
        var listaBloqueios = [];    // Array de objetos
        var cacheSubespecialidades = null;
        var cacheMotivos = null;

        // --- INICIALIZAÇÃO ---
        $(function () {
            // 1. Carregamentos Iniciais
            CarregarDropdownMeses();
            CarregarMotivosBackend();

            // 2. Eventos de Change
            $('#<%= ddlClinica.ClientID %>').change(function () {
                var cod = $(this).val();
                CarregarProfissionais(cod);
                CarregarSubespecialidades(cod);
                $('#newSub').val(''); // Limpa subespecialidade ao trocar clínica
            });

            $('#selProfissional').change(function () {
                $('#<%= hdnCodProfissional.ClientID %>').val($(this).val());
                $('#<%= hdnNomeProfissional.ClientID %>').val($(this).find("option:selected").text());
            });

            // 3. Eventos de Adição (Botões)
            $('#btnAdicionarMes').click(AdicionarMes);
            $('#btnAddItemHorario').click(AdicionarHorarioLista);
            $('#btnAddItemBloqueio').click(AdicionarBloqueioLista);

            // 4. Checar Edição (Carrega dados se existir ID)
            var idEdicao = $('#<%= hdnIdPreAgendamento.ClientID %>').val();
            if (idEdicao) {
                // Pequeno delay para garantir que PageMethods estejam prontos
                setTimeout(CarregarDadosEdicao, 500);
            }
        });


        // =========================================================
        // LÓGICA DE MESES (TAGS)
        // =========================================================
        function CarregarDropdownMeses() {
            var ddl = $('#ddlMesesDisponiveis');
            ddl.empty().append(new Option('Selecione...', ''));

            var hoje = new Date();
            var dataCursor = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 1);

            for (var i = 0; i < 12; i++) {
                var mes = dataCursor.getMonth() + 1;
                var ano = dataCursor.getFullYear();
                var chave = ano + '-' + (mes < 10 ? '0' + mes : mes);
                var label = dataCursor.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
                label = label.charAt(0).toUpperCase() + label.slice(1); // Capitaliza

                ddl.append(new Option(label, chave));
                dataCursor.setMonth(dataCursor.getMonth() + 1);
            }
        }

        function AdicionarMes() {
            var ddl = $('#ddlMesesDisponiveis');
            var chave = ddl.val();
            var label = ddl.find('option:selected').text();

            if (!chave) return;
            if (listaMeses.has(chave)) { alert('Este mês já foi adicionado.'); return; }

            listaMeses.set(chave, label);
            RenderizarMeses();
            ddl.val('');
        }

        function RenderizarMeses() {
            var container = $('#containerMesesTags');
            var hdn = $('#<%= hdnMesesSelecionados.ClientID %>');
            container.empty();

            if (listaMeses.size === 0) {
                container.append('<span class="text-muted small ms-2" id="lblNenhumMes">Nenhum mês selecionado.</span>');
                hdn.val('');
                return;
            }

            listaMeses.forEach(function (label, chave) {
                var tag = $('<div class="mes-tag">' + label + ' <i class="fas fa-times btn-remove" title="Remover"></i></div>');
                tag.find('.btn-remove').click(function () {
                    listaMeses.delete(chave);
                    RenderizarMeses();
                });
                container.append(tag);
            });

            // Atualiza Hidden Field para o Server (Formato: 2026-01,2026-02)
            hdn.val(Array.from(listaMeses.keys()).join(','));
        }

        // =========================================================
        // LÓGICA DE HORÁRIOS (TABELA)
        // =========================================================
        function AdicionarHorarioLista() {
            // Ler Inputs
            var dia = $('#newDia').val();
            var hora = $('#newHora').val();
            var novas = $('#newNovas').val();
            var retorno = $('#newRetorno').val();
            var subId = $('#newSub').val();
            var subTxt = subId ? $('#newSub option:selected').text() : "";

            // Validação Obrigatória (Dia, Hora e Subespecialidade)
            if (!dia || !hora || !subId) {
                alert('Preencha Dia, Horário e selecione a Subespecialidade.');
                return;
            }

            if ((!novas || novas == "0") && (!retorno || retorno == "0")) {
                alert('Informe ao menos uma quantidade de consultas (Novas ou Retorno).'); return;
            }

            // Objeto
            var item = {
                DiaSemana: dia,
                Horario: hora,
                ConsultasNovas: novas || "0",
                ConsultasRetorno: retorno || "0",
                CodSubespecialidade: subId,
                SubespecialidadeTexto: subTxt
            };

            // Adicionar e Limpar
            listaHorarios.push(item);
            RenderizarTabelaHorarios();

            // Limpar campos
            $('#newDia').val('').focus();
            $('#newNovas').val('');
            $('#newRetorno').val('');
            $('#newHora').val('');
            // Nota: Não limpamos Subespecialidade por conveniência do usuário
        }

        function RenderizarTabelaHorarios() {
            var tbody = $('#tblHorarios tbody');
            tbody.empty();

            if (listaHorarios.length === 0) {
                $('#emptyHorarios').show();
                $('#tblHorarios').hide();
                $('#<%= hdnBlocosJson.ClientID %>').val('');
                return;
            }

            $('#emptyHorarios').hide();
            $('#tblHorarios').show();

            listaHorarios.forEach(function (item, index) {
                var tr = $('<tr>');
                tr.append('<td><strong>' + item.DiaSemana + '</strong></td>');
                tr.append('<td>' + item.Horario + '</td>');
                tr.append('<td>' + item.SubespecialidadeTexto + '</td>');

                var badgeNovas = item.ConsultasNovas > 0 ? '<span class="badge bg-primary rounded-pill">' + item.ConsultasNovas + '</span>' : '-';
                var badgeRetorno = item.ConsultasRetorno > 0 ? '<span class="badge bg-info text-dark rounded-pill">' + item.ConsultasRetorno + '</span>' : '-';

                tr.append('<td class="text-center">' + badgeNovas + '</td>');
                tr.append('<td class="text-center">' + badgeRetorno + '</td>');

                var tdAcoes = $('<td class="text-end"></td>');
                var btnDel = $('<button type="button" class="btn-action-icon"><i class="fas fa-trash-alt"></i></button>');
                btnDel.click(function () {
                    listaHorarios.splice(index, 1);
                    RenderizarTabelaHorarios();
                });
                tdAcoes.append(btnDel);
                tr.append(tdAcoes);

                tbody.append(tr);
            });

            $('#<%= hdnBlocosJson.ClientID %>').val(JSON.stringify(listaHorarios));
        }

        // =========================================================
        // LÓGICA DE BLOQUEIOS (TABELA)
        // =========================================================
        function CarregarMotivosBackend() {
            PageMethods.ListarMotivosBloqueio(function (res) {
                cacheMotivos = res;
                var ddl = $('#newBloqMotivo');
                ddl.empty().append(new Option('Selecione...', ''));
                $.each(res, function (i, m) {
                    ddl.append(new Option(m.NmMotivo, m.CodMotivo));
                });
            });
        }

        // Função de Validação de Datas do Bloqueio
        function validarDataNoPeriodo(dataIso) {
            if (!dataIso || dataIso.length < 7) return false;

            // Pega "yyyy-mm"
            var anoMes = dataIso.substring(0, 7);

            // Verifica no Map visual
            if (listaMeses.size > 0) return listaMeses.has(anoMes);

            // Fallback: Verifica no HiddenField
            var hdnValor = $('[id$=hdnMesesSelecionados]').val();
            if (hdnValor) {
                var arrayMeses = hdnValor.split(',');
                return arrayMeses.indexOf(anoMes) > -1;
            }
            return false;
        }

        function AdicionarBloqueioLista() {
            var de = $('#newBloqDe').val();
            var ate = $('#newBloqAte').val();
            var motivoId = $('#newBloqMotivo').val();
            var motivoTxt = $('#newBloqMotivo option:selected').text();

            // 1. Validação Básica
            if (!de || !ate || !motivoId) {
                alert('Preencha as datas De/Até e o Motivo.');
                return;
            }

            // 2. Validação Lógica (Início > Fim)
            if (de > ate) {
                alert('A data inicial não pode ser maior que a final.');
                // Limpa os campos para o usuário corrigir
                $('#newBloqDe').val('');
                $('#newBloqAte').val('');
                $('#newBloqMotivo').val('');

                return; // Sai da função aqui
            }

            // 3. VALIDAÇÃO: Verifica se as datas pertencem aos meses selecionados
            var fmtData = function (d) { return d.split('-').reverse().join('/'); };

            if (!validarDataNoPeriodo(de)) {
                alert('A data INICIAL (' + fmtData(de) + ') não pertence aos meses selecionados na agenda.');

                // --- AQUI É O LUGAR CORRETO PARA LIMPAR ---
                $('#newBloqDe').val('');
                $('#newBloqAte').val('');
                $('#newBloqMotivo').val('');

                // ------------------------------------------

                return; // Sai da função
            }

            if (!validarDataNoPeriodo(ate)) {
                alert('A data FINAL (' + fmtData(ate) + ') não pertence aos meses selecionados na agenda.');

                // --- AQUI É O LUGAR CORRETO PARA LIMPAR ---
                $('#newBloqDe').val('');
                $('#newBloqAte').val('');
                $('#newBloqMotivo').val('');

                // ------------------------------------------

                return; // Sai da função
            }

            // Se passou por tudo, adiciona na lista
            var item = {
                De: de,
                Ate: ate,
                CodMotivo: motivoId,
                MotivoTexto: motivoTxt
            };

            listaBloqueios.push(item);
            RenderizarTabelaBloqueios();

            // Limpar campos APÓS adicionar com sucesso (para nova inserção)
            $('#newBloqDe').val('');
            $('#newBloqAte').val('');
            $('#newBloqMotivo').val('');
        }

        function RenderizarTabelaBloqueios() {
            var tbody = $('#tblBloqueios tbody');
            tbody.empty();

            if (listaBloqueios.length === 0) {
                $('#emptyBloqueios').show();
                $('#tblBloqueios').hide();
                $('#<%= hdnBloqueiosJson.ClientID %>').val('');
                return;
            }

            $('#emptyBloqueios').hide();
            $('#tblBloqueios').show();

            listaBloqueios.forEach(function (item, index) {
                var fmtData = function (d) { return d.split('-').reverse().join('/'); };

                var tr = $('<tr>');
                tr.append('<td>' + fmtData(item.De) + '</td>');
                tr.append('<td>' + fmtData(item.Ate) + '</td>');
                tr.append('<td>' + item.MotivoTexto + '</td>');

                var tdAcoes = $('<td class="text-end"></td>');
                var btnDel = $('<button type="button" class="btn-action-icon"><i class="fas fa-trash-alt"></i></button>');
                btnDel.click(function () {
                    listaBloqueios.splice(index, 1);
                    RenderizarTabelaBloqueios();
                });
                tdAcoes.append(btnDel);
                tr.append(tdAcoes);

                tbody.append(tr);
            });

            $('#<%= hdnBloqueiosJson.ClientID %>').val(JSON.stringify(listaBloqueios));
        }

        // =========================================================
        // AJAX AUXILIARES
        // =========================================================
        function CarregarProfissionais(codClinica) {
            var sel = $('#selProfissional');
            sel.empty().append(new Option('Carregando...', ''));

            if (!codClinica) { sel.empty().append(new Option('Selecione...', '')); return; }

            PageMethods.ListarProfissionais(parseInt(codClinica), function (res) {
                sel.empty().append(new Option('Selecione...', ''));
                $.each(res, function (i, p) {
                    sel.append(new Option(p.NomeProfissional, p.CodProfissional));
                });
                // Restaurar valor se for edição
                var salvo = $('#<%= hdnCodProfissional.ClientID %>').val();
                if (salvo) sel.val(salvo);
            });
        }

        function CarregarSubespecialidades(codClinica) {
            var sel = $('#newSub');
            sel.empty().append(new Option('Carregando...', ''));

            if (!codClinica) {
                sel.empty().append(new Option('Selecione...', ''));
                return;
            }

            PageMethods.ListarSubespecialidades(parseInt(codClinica), function (res) {
                cacheSubespecialidades = res;
                sel.empty().append(new Option('Selecione...', ''));
                $.each(res, function (i, s) {
                    sel.append(new Option(s.NomeSubespecialidade, s.CodSubespecialidade));
                });
            });
        }

        // =========================================================
        // VALIDAÇÃO FINAL (SAVE)
        // =========================================================
        function ValidarAntesDeSalvar() {
            var erros = [];

            if (!$('#<%= ddlClinica.ClientID %>').val()) erros.push('Selecione a Clínica.');
            if (!$('#<%= hdnCodProfissional.ClientID %>').val()) erros.push('Selecione o Profissional.');
            if (listaMeses.size === 0) erros.push('Adicione pelo menos um mês à agenda.');
            if (listaHorarios.length === 0) erros.push('Adicione pelo menos um horário de atendimento.');

            if (erros.length > 0) {
                var ul = $('#listaErros');
                ul.empty();
                erros.forEach(function (e) { ul.append('<li>' + e + '</li>'); });
                var modal = new bootstrap.Modal(document.getElementById('errosModal'));
                modal.show();
                return false;
            }
            return true;
        }

        // =========================================================
        // CARREGAR EDIÇÃO
        // =========================================================
        function CarregarDadosEdicao() {
            // 1. Trigger mudança de clínica
            var codClinica = $('#<%= ddlClinica.ClientID %>').val();
            if (codClinica) {
                CarregarProfissionais(codClinica);
                CarregarSubespecialidades(codClinica);
            }

            // 2. Meses (COM CORREÇÃO VISUAL PARA NOME POR EXTENSO)
            var strMeses = $('#<%= hdnMesesSelecionados.ClientID %>').val();
            if (strMeses) {
                listaMeses.clear();
                strMeses.split(',').forEach(function (m) {
                    m = m.trim();
                    if (m && m.indexOf('-') > -1) {
                        // Converte "2026-01" para "Janeiro de 2026"
                        var partes = m.split('-');
                        var ano = parseInt(partes[0]);
                        var mes = parseInt(partes[1]);
                        var dataObj = new Date(ano, mes - 1, 1);
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

            // 4. Bloqueios (COM CORREÇÃO PARA MOTIVO NULL)
            var jsonB = $('#<%= hdnBloqueiosJson.ClientID %>').val();
            if (jsonB) {
                listaBloqueios = JSON.parse(jsonB);

                // Fallback caso o Texto venha nulo
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
                    }
                }
                RenderizarTabelaBloqueios();
            }
        }

    </script>
</asp:Content>
