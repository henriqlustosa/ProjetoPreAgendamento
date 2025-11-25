<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="preagendamento.aspx.cs" Inherits="publico_preagendamento"
    Title="Pré-Agendamento - Cadastro" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        body {
            background: #f8f9fa;
            font-family: 'Segoe UI', sans-serif;
            font-size: 1rem;
        }

        .preagendamento-container {
            padding: 20px 0 40px;
        }

        .page-title-card {
            border-radius: 8px 8px 0 0;
            background: #2A3F54;
            color: #fff;
            padding: 16px 22px;
            font-weight: 600;
            font-size: 1.1rem;
        }

        .card-block {
            background: #ffffff;
            border-radius: 0 0 8px 8px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.08);
            padding: 22px 24px 12px;
        }

        .section-card {
            background: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.04);
            padding: 18px 20px 12px;
            margin-bottom: 18px;
        }

        .section-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 12px;
            border-bottom: 1px solid #e2e6ea;
            padding-bottom: 6px;
        }

        .section-title {
            font-size: 1.15rem;
            font-weight: 600;
            color: #2A3F54;
            margin: 0;
        }

            .section-title i {
                margin-right: 6px;
            }

        .form-group label {
            font-size: .95rem;
            font-weight: 600;
            color: #495057;
            margin-bottom: 2px;
        }

        .form-control, select.form-control {
            border-radius: 6px;
            font-size: .95rem;
            padding: .45rem .6rem;
        }

            .form-control:focus {
                box-shadow: 0 0 0 0.15rem rgba(42, 63, 84, .25);
                border-color: #2A3F54;
            }

        .small-help {
            font-size: .8rem;
            color: #868e96;
        }

        .day-box {
            border: 1px solid #dee2e6;
            border-radius: 8px;
            padding: 12px 12px 8px;
            margin-bottom: 10px;
            background: #fdfdff;
        }

        .day-title {
            font-weight: 600;
            margin-bottom: 10px;
            font-size: .95rem;
            color: #2A3F54;
        }

        .btn-remove-day {
            padding: 0.15rem .5rem;
            font-size: .8rem;
        }

        .bloqueios-header {
            font-size: .95rem;
            font-weight: 600;
            margin-bottom: 6px;
        }

        .btn-add-bloqueio, .btn-add-day, .btn-add-mes {
            font-size: .9rem;
            min-width: 180px;
            padding: .25rem .75rem;
        }

        .section-header .btn-add-bloqueio, .section-header .btn-add-day, .section-header .btn-add-mes {
            margin-left: 12px;
        }

        .actions-bar {
            margin-top: 14px;
            padding-top: 14px;
            border-top: 1px solid #dee2e6;
            display: flex;
            justify-content: flex-end;
            gap: 8px;
        }

        .preagendamento-container .btn-primary {
            background-color: #2A3F54;
            border-color: #2A3F54;
        }

            .preagendamento-container .btn-primary:hover {
                background-color: #1f2f40;
                border-color: #1f2f40;
            }

        .preagendamento-container .btn-outline-primary {
            color: #2A3F54;
            border-color: #2A3F54;
        }

            .preagendamento-container .btn-outline-primary:hover {
                background-color: #2A3F54;
                color: #fff;
            }

        .preagendamento-container .btn-outline-secondary {
            color: #1ABB9C;
            border-color: #1ABB9C;
        }

            .preagendamento-container .btn-outline-secondary:hover {
                background-color: #1ABB9C;
                color: #fff;
            }

        .preagendamento-container .is-invalid {
            border-color: #dc3545 !important;
            box-shadow: 0 0 0 0.15rem rgba(220, 53, 69, .25);
        }

        .preagendamento-container .campo-obrigatorio {
            color: #dc3545;
            font-size: 0.8rem;
            margin-left: 4px;
        }

        .preagendamento-container .day-box.erro-bloco {
            border-color: #dc3545;
            box-shadow: 0 0 0 0.08rem rgba(220, 53, 69, .25);
        }

        .preagendamento-container .bloco-bloqueio.erro-bloco {
            border: 1px solid #dc3545;
            border-radius: 6px;
            box-shadow: 0 0 0 0.08rem rgba(220, 53, 69, .25);
            padding-top: 4px;
            padding-bottom: 4px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

    <div class="container-fluid preagendamento-container">
        <div class="row justify-content-center">
            <div class="col-xl-10 col-lg-11">

                <div class="page-title-card">
                    <asp:Label ID="lblTituloPagina" runat="server" Text="HSPM Pré-Agendamento - Agendas Médicas"></asp:Label>
                </div>

                <div class="card-block">
                    <div class="section-card">
                        <div class="section-header">
                            <div class="section-title"><i class="fas fa-user-md"></i>Dados Gerais</div>
                        </div>
                        <div class="form-row">
                            <div class="form-group col-md-4">
                                <label>Data de Preenchimento</label>
                                <asp:TextBox ID="txtDataPreenchimento" CssClass="form-control" TextMode="SingleLine" runat="server" />
                                <asp:HiddenField ID="hdnIdPreAgendamento" runat="server" />
                            </div>
                            <div class="form-group col-md-4">
                                <label>Clínica <span class="campo-obrigatorio">*</span></label>
                                <asp:DropDownList ID="ddlClinica" CssClass="form-control" runat="server" />
                            </div>
                            <div class="form-group col-md-4">
                                <label>Nome do Profissional <span class="campo-obrigatorio">*</span></label>
                                <select id="selProfissional" class="form-control">
                                    <option value="">Selecione...</option>
                                </select>
                                <asp:HiddenField ID="hdnCodProfissional" runat="server" />
                                <asp:HiddenField ID="hdnNomeProfissional" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="section-card">
                        <div class="section-header">
                            <div class="section-title"><i class="far fa-calendar-alt"></i>Meses para Pré-Agendamento</div>
                            <button type="button" id="btnAdicionarMes" class="btn btn-outline-primary btn-sm btn-add-mes">
                                <i class="fas fa-plus-circle"></i>Adicionar mês
                            </button>
                        </div>
                        <div class="form-row mb-2">
                            <div class="form-group col-md-4">
                                <label>Adicionar os meses para agenda: <span class="campo-obrigatorio">*</span></label>
                                <select id="ddlMesesDisponiveis" class="form-control"></select>
                            </div>
                        </div>
                        <div class="row" id="containerMesesSelecionados"></div>
                        <asp:HiddenField ID="hdnMesesSelecionados" runat="server" />
                    </div>

                    <div class="section-card">
                        <div class="section-header">
                            <div class="section-title"><i class="far fa-clock"></i>Horários por Dia da Semana</div>
                            <button type="button" id="btnAddBloco" class="btn btn-outline-primary btn-sm btn-add-day">
                                <i class="fas fa-plus-circle"></i>Adicionar dia
                            </button>
                        </div>
                        <div id="blocosContainer" class="row"></div>
                        <asp:HiddenField ID="hdnBlocosJson" runat="server" />
                    </div>

                    <div class="section-card">
                        <div class="section-header">
                            <div class="section-title"><i class="fas fa-info-circle"></i>Dados Complementares</div>
                        </div>
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <div class="bloqueios-header"><i class="fas fa-ban"></i>Bloqueios</div>
                            <button type="button" id="btnAddBloqueio" class="btn btn-outline-secondary btn-sm btn-add-bloqueio">
                                <i class="fas fa-plus-circle"></i>Adicionar bloqueio
                            </button>
                        </div>
                        <div id="bloqueiosContainer" class="mb-2"></div>
                        <asp:HiddenField ID="hdnBloqueiosJson" runat="server" />
                        <div class="form-group mt-3">
                            <label>Observações</label>
                            <asp:TextBox ID="txtObservacoes" CssClass="form-control" TextMode="MultiLine" Rows="4" runat="server" />
                        </div>
                    </div>

                    <div class="actions-bar">
                        <asp:Button ID="btnSalvar" CssClass="btn btn-primary" Text="Salvar" OnClick="btnSalvar_Click" OnClientClick="return PrepararBlocos();" runat="server" />
                        <asp:Button ID="btnLimpar" CssClass="btn btn-secondary" Text="Limpar" OnClick="btnLimpar_Click" runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="errosModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title"><i class="fas fa-exclamation-triangle me-2"></i>Erros de validação</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Fechar"></button>
                </div>
                <div class="modal-body">
                    <p>Corrija os campos abaixo antes de salvar:</p>
                    <ul id="listaErros" class="mb-0"></ul>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Fechar</button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        // --- Variáveis Globais ---
        var blocoIndex = 0;
        var bloqueioIndex = 0;
        var subespecialidadesCache = null;
        var mesesSelecionados = new Map(); // Mapa global para controlar meses

        // ============================================================
        // LÓGICA DE MESES (Movida para escopo acessível)
        // ============================================================
        function preencherDropdownMeses() {
            var ddlMeses = document.getElementById('ddlMesesDisponiveis');
            var txtData = document.getElementById('<%= txtDataPreenchimento.ClientID %>');

            ddlMeses.innerHTML = '';
            ddlMeses.appendChild(new Option('Selecione...', ''));

            var hoje = new Date();
            // Tenta pegar da data preenchida, senão usa hoje
            if (txtData && txtData.value) {
                var partes = txtData.value.split('-');
                if (partes.length === 3) hoje = new Date(partes[0], partes[1] - 1, partes[2]);
            }

            var dataAtual = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 1);

            for (var i = 0; i < 12; i++) {
                var mes = dataAtual.getMonth() + 1;
                var ano = dataAtual.getFullYear();
                var mesStr = (mes < 10 ? '0' + mes : mes.toString());
                var chave = ano + '-' + mesStr;
                // Formata label (Ex: Novembro de 2025)
                var label = dataAtual.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
                // Capitaliza primeira letra
                label = label.charAt(0).toUpperCase() + label.slice(1);

                ddlMeses.appendChild(new Option(label, chave));
                dataAtual.setMonth(dataAtual.getMonth() + 1);
            }
        }

        function atualizarHiddenFieldMeses() {
            var hdn = document.getElementById('<%= hdnMesesSelecionados.ClientID %>');
            hdn.value = Array.from(mesesSelecionados.keys()).join(',');
        }

        function criarCardMes(chave, label) {
            var container = document.getElementById('containerMesesSelecionados');

            // Evita duplicatas visuais
            if (container.querySelector('[data-mes="' + chave + '"]')) return;

            var div = document.createElement('div');
            div.className = 'col-md-3 col-sm-4 mb-3';
            div.setAttribute('data-mes', chave);
            div.innerHTML =
                '<div class="card shadow-sm h-100">' +
                '  <div class="card-body d-flex flex-column">' +
                '    <div class="d-flex justify-content-between align-items-start mb-2">' +
                '      <div>' +
                '        <div class="fw-bold">' + label + '</div>' +
                '        <small class="text-muted">Pré-agendamento</small>' +
                '      </div>' +
                '      <button type="button" class="btn btn-sm btn-link text-danger p-0 btn-remover-mes" title="Remover mês">' +
                '        <i class="fas fa-times"></i>' +
                '      </button>' +
                '    </div>' +
                '  </div>' +
                '</div>';
            container.appendChild(div);
        }

        // ============================================================
        // AJAX E AUXILIARES
        // ============================================================
        function carregarProfissionaisPorClinica(codPreSelecionado = null) {
            var ddlClinica = $('#<%= ddlClinica.ClientID %>');
            var selProf = $('#selProfissional');
            var codEsp = ddlClinica.val();

            if (!codPreSelecionado) {
                $('#<%= hdnCodProfissional.ClientID %>').val('');
                $('#<%= hdnNomeProfissional.ClientID %>').val('');
                selProf.empty().append($('<option/>', { value: '', text: 'Selecione...' }));
            }

            if (!codEsp) return;

            PageMethods.ListarProfissionais(parseInt(codEsp),
                function (result) {
                    selProf.empty().append($('<option/>', { value: '', text: 'Selecione...' }));
                    for (var i = 0; i < result.length; i++) {
                        selProf.append($('<option/>', {
                            value: result[i].CodProfissional,
                            text: result[i].NomeProfissional
                        }));
                    }
                    if (codPreSelecionado) {
                        selProf.val(codPreSelecionado);
                        if (!selProf.val()) {
                            var nomeSalvo = $('#<%= hdnNomeProfissional.ClientID %>').val();
                            selProf.append($('<option/>', { value: codPreSelecionado, text: nomeSalvo, selected: true }));
                        }
                        // --- CORREÇÃO AQUI: ---
                        // Atualiza o HiddenField do NOME imediatamente após selecionar o valor via código
                        var nomeSelecionado = selProf.find("option:selected").text();
                        $('#<%= hdnNomeProfissional.ClientID %>').val(nomeSelecionado);
                        // ----------------------
                    }
                },
                function (err) { alert('Erro: ' + err.get_message()); }
            );
        }

        function preencherSelectSubespecialidade($ddl, lista) {
            $ddl.empty().append($('<option>').val('').text('Selecione...'));
            $.each(lista, function (i, item) {
                $ddl.append($('<option>').val(item.CodSubespecialidade).text(item.NomeSubespecialidade));
            });
        }

        function carregarSubespecialidades(codEspec) {
            if (!codEspec) {
                $('.campoSubespecialidade').empty().append($('<option>').val('').text('Selecione...'));
                subespecialidadesCache = null; return;
            }
            PageMethods.ListarSubespecialidades(parseInt(codEspec), function (lista) {
                subespecialidadesCache = lista;
                $('.campoSubespecialidade').each(function () {
                    var $ddl = $(this);
                    var valorParaSelecionar = $ddl.val() || $ddl.attr('data-selected');
                    preencherSelectSubespecialidade($ddl, lista);
                    if (valorParaSelecionar) $ddl.val(valorParaSelecionar);
                });
            }, function (e) { console.log(e); });
        }

        // ============================================================
        // GERADORES DE HTML (Blocos e Bloqueios)
        // ============================================================
        function criarBlocoHtml(index) {
            return '<div class="col-md-4 bloco-dia-wrapper" data-index="' + index + '">' +
                '  <div class="day-box bloco-dia">' +
                '    <div class="day-title">Dia da Semana</div>' +
                '    <label>Dia da Semana</label>' +
                '    <select class="form-control campo-dia-semana">' +
                '      <option value="">Selecione...</option>' +
                '      <option value="Segunda">Segunda</option>' +
                '      <option value="Terça">Terça</option>' +
                '      <option value="Quarta">Quarta</option>' +
                '      <option value="Quinta">Quinta</option>' +
                '      <option value="Sexta">Sexta</option>' +
                '    </select>' +
                '    <label class="mt-2">Horário</label>' +
                '    <input type="time" class="form-control campo-horario" />' +
                '    <label class="mt-2">Consultas Novas</label>' +
                '    <input type="text" class="form-control campo-consultas-novas" oninput="this.value = this.value.replace(/[^0-9]/g, \'\')" placeholder="0" />' +
                '    <label class="mt-2">Consultas Retorno</label>' +
                '    <input type="text" class="form-control campo-consultas-retorno" oninput="this.value = this.value.replace(/[^0-9]/g, \'\')" placeholder="0" />' +
                '    <label class="mt-2">Subespecialidade</label>' +
                '    <select class="form-control campoSubespecialidade">' +
                '      <option value="">Selecione...</option>' +
                '    </select>' +
                '    <button type="button" class="btn btn-sm btn-outline-danger mt-2 btn-remover-bloco"><i class="fas fa-times"></i> Remover</button>' +
                '  </div>' +
                '</div>';
        }

        function adicionarBloco() {
            var html = criarBlocoHtml(blocoIndex++);
            $('#blocosContainer').append(html);
            var $novoSelect = $('#blocosContainer .bloco-dia-wrapper').last().find('.campoSubespecialidade');
            if (subespecialidadesCache && subespecialidadesCache.length > 0) {
                preencherSelectSubespecialidade($novoSelect, subespecialidadesCache);
            } else {
                var codClinica = $('#<%= ddlClinica.ClientID %>').val();
                if (codClinica && !subespecialidadesCache) carregarSubespecialidades(codClinica);
            }
        }

        function criarBloqueioHtml(index) {
            return '<div class="row bloco-bloqueio mb-2" data-index="' + index + '">' +
                '  <div class="col-md-3"><label>De</label><input type="date" class="form-control campo-bloq-de" /></div>' +
                '  <div class="col-md-3"><label>Até</label><input type="date" class="form-control campo-bloq-ate" /></div>' +
                '  <div class="col-md-5"><label>Motivo</label><select class="form-control campo-bloq-motivo">' +
                '      <option value="">Selecione...</option>' +
                '      <option value="Férias">Férias</option>' +
                '      <option value="Congresso">Congresso</option>' +
                '      <option value="Ausência Justificada">Ausência Justificada</option>' +
                '      <option value="Agenda Suspensa">Agenda Suspensa</option>' +
                '      <option value="Bloqueio Administrativo">Bloqueio Administrativo</option>' +
                '  </select></div>' +
                '  <div class="col-md-1 d-flex align-items-end"><button type="button" class="btn btn-sm btn-outline-danger btn-remover-bloqueio"><i class="fas fa-times"></i></button></div>' +
                '</div>';
        }

        function adicionarBloqueio() {
            var html = criarBloqueioHtml(bloqueioIndex++);
            $('#bloqueiosContainer').append(html);
            var $novo = $('#bloqueiosContainer .bloco-bloqueio').last();
            $novo.find('.campo-bloq-de, .campo-bloq-ate, .campo-bloq-motivo').on('blur change', function () {
                var $campo = $(this);
                if (!$campo.val()) {
                    $campo.addClass('is-invalid');
                    $novo.addClass('erro-bloco');
                } else {
                    $campo.removeClass('is-invalid');
                    if ($novo.find('.is-invalid').length === 0) $novo.removeClass('erro-bloco');
                }
            });
        }

        // ============================================================
        // VALIDAÇÃO E PREPARAÇÃO (Save)
        // ============================================================
        function limparErrosCamposObrigatorios() {
            $('.is-invalid').removeClass('is-invalid');
            $('.erro-bloco').removeClass('erro-bloco');
        }

        function exibirErrosModal(erros) {
            var lista = document.getElementById('listaErros');
            var modal = document.getElementById('errosModal');
            if (!lista || !modal) { alert(erros.join('\n')); return; }
            lista.innerHTML = '';
            erros.forEach(function (msg) {
                var li = document.createElement('li');
                li.textContent = msg;
                lista.appendChild(li);
            });
            var bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        }

        function PrepararBlocos() {
            var erros = [];
            var blocos = [];
            var bloqueios = [];
            var temBlocoValido = false;
            var combinacoesBlocos = new Set();

            limparErrosCamposObrigatorios();

            var ddlClinica = $('#<%= ddlClinica.ClientID %>');
            var hdnCodProf = $('#<%= hdnCodProfissional.ClientID %>');
            var hdnMeses = $('#<%= hdnMesesSelecionados.ClientID %>');

            // Atualiza Hidden Meses antes de validar
            atualizarHiddenFieldMeses();
            var mesesVal = hdnMeses.val();

            if (!ddlClinica.val()) { erros.push('Informe a Clínica nos Dados Gerais.'); ddlClinica.addClass('is-invalid'); }
            if (!hdnCodProf.val()) { erros.push('Informe o Nome do Profissional nos Dados Gerais.'); $('#selProfissional').addClass('is-invalid'); }
            if (!$('#<%= txtDataPreenchimento.ClientID %>').val()) erros.push('Data de Preenchimento não informada.');
            
            if (!mesesVal) erros.push('Informe pelo menos um mês em "Meses para Pré-Agendamento".');

            // Validação Blocos
            $('#blocosContainer .bloco-dia').each(function (idx) {
                var $b = $(this);
                var diaSemana = $b.find('.campo-dia-semana').val();
                var horario = $b.find('.campo-horario').val();
                var cNovas = $b.find('.campo-consultas-novas').val();
                var cRetorno = $b.find('.campo-consultas-retorno').val();
                var subId = $b.find('.campoSubespecialidade').val();
                var subTexto = $b.find('.campoSubespecialidade option:selected').text();

                if (!diaSemana && !horario && !cNovas && !cRetorno && !subId) return;

                var prefixo = 'Bloco ' + (idx + 1) + ': ';
                var valido = true;

                if (!diaSemana) { erros.push(prefixo + 'selecione o Dia.'); valido = false; $b.find('.campo-dia-semana').addClass('is-invalid'); }
                if (!horario) { erros.push(prefixo + 'informe o Horário.'); valido = false; $b.find('.campo-horario').addClass('is-invalid'); }
                if ((!cNovas && cNovas !== "0") && (!cRetorno && cRetorno !== "0")) {
                    erros.push(prefixo + 'informe Consultas.'); valido = false; $b.find('.campo-consultas-novas, .campo-consultas-retorno').addClass('is-invalid');
                }
                if (!subId) { erros.push(prefixo + 'selecione a Subespecialidade.'); valido = false; $b.find('.campoSubespecialidade').addClass('is-invalid'); }

                if (valido) {
                    var chave = diaSemana + '|' + horario + '|' + subId;
                    if (combinacoesBlocos.has(chave)) { erros.push(prefixo + 'bloco duplicado.'); valido = false; $b.addClass('erro-bloco'); }
                    else combinacoesBlocos.add(chave);
                }

                if (valido) {
                    temBlocoValido = true;
                    blocos.push({
                        DiaSemana: diaSemana,
                        Horario: horario,
                        ConsultasNovas: cNovas,
                        ConsultasRetorno: cRetorno,
                        CodSubespecialidade: subId,
                        SubespecialidadeTexto: subTexto
                    });
                } else {
                    $b.addClass('erro-bloco');
                }
            });

            if (!temBlocoValido) erros.push('Informe pelo menos um horário completo.');

            // Validação Bloqueios
            $('#bloqueiosContainer .bloco-bloqueio').each(function (idx) {
                var $b = $(this);
                var de = $b.find('.campo-bloq-de').val();
                var ate = $b.find('.campo-bloq-ate').val();
                var motivo = $b.find('.campo-bloq-motivo').val();
                var prefixo = 'Bloqueio ' + (idx + 1) + ': ';
                var erroLinha = false;

                if (!de && !ate && !motivo) { erros.push(prefixo + 'preencha ou remova.'); erroLinha = true; $b.find('input, select').addClass('is-invalid'); }
                else {
                    if (!de) { erros.push(prefixo + 'falta "De".'); erroLinha = true; $b.find('.campo-bloq-de').addClass('is-invalid'); }
                    if (!ate) { erros.push(prefixo + 'falta "Até".'); erroLinha = true; $b.find('.campo-bloq-ate').addClass('is-invalid'); }
                    if (!motivo) { erros.push(prefixo + 'falta "Motivo".'); erroLinha = true; $b.find('.campo-bloq-motivo').addClass('is-invalid'); }
                    if (!erroLinha && de && ate) {
                        if (de > ate) { erros.push(prefixo + 'Data inicial maior que final.'); erroLinha = true; $b.find('input[type=date]').addClass('is-invalid'); }
                    }
                }

                if (!erroLinha) bloqueios.push({ De: de, Ate: ate, Motivo: motivo });
                else $b.addClass('erro-bloco');
            });

            if (erros.length > 0) { exibirErrosModal(erros); return false; }

            $('#<%= hdnBlocosJson.ClientID %>').val(JSON.stringify(blocos));
            $('#<%= hdnBloqueiosJson.ClientID %>').val(JSON.stringify(bloqueios));
            return true;
        }

        // ============================================================
        // CARREGAR DADOS EDIÇÃO
        // ============================================================
        function carregarDadosEdicao() {
            // 1. Carrega Profissional
            var codClinica = $('#<%= ddlClinica.ClientID %>').val();
            var codProf = $('#<%= hdnCodProfissional.ClientID %>').val();

            if (codClinica && codProf) {
                carregarProfissionaisPorClinica(codProf);
                carregarSubespecialidades(codClinica);
            }

            // 2. Recria Blocos
            var jsonBlocos = $('#<%= hdnBlocosJson.ClientID %>').val();
            if (jsonBlocos && jsonBlocos !== '[]') {
                var blocos = JSON.parse(jsonBlocos);
                $('#blocosContainer').empty();
                blocoIndex = 0;

                blocos.forEach(function (b) {
                    var html = criarBlocoHtml(blocoIndex);
                    $('#blocosContainer').append(html);
                    var $row = $('#blocosContainer .bloco-dia-wrapper').last();

                    $row.find('.campo-dia-semana').val(b.DiaSemana || b.diaSemana);
                    $row.find('.campo-horario').val(b.Horario || b.horario);
                    $row.find('.campo-consultas-novas').val(b.ConsultasNovas || b.consultasNovas);
                    $row.find('.campo-consultas-retorno').val(b.ConsultasRetorno || b.consultasRetorno);

                    var codSub = b.CodSubespecialidade || b.codSubespecialidade;
                    var txtSub = b.SubespecialidadeTexto || b.subespecialidadeTexto || "Selecionada";
                    if (codSub) {
                        var $ddl = $row.find('.campoSubespecialidade');
                        $ddl.attr('data-selected', codSub);
                        if (subespecialidadesCache) {
                            preencherSelectSubespecialidade($ddl, subespecialidadesCache);
                            $ddl.val(codSub);
                        } else {
                            $ddl.empty().append(new Option(txtSub, codSub, true, true));
                        }
                    }
                    blocoIndex++;
                });
            }

            // 3. Recria Bloqueios
            var jsonBloqueios = $('#<%= hdnBloqueiosJson.ClientID %>').val();
            if (jsonBloqueios && jsonBloqueios !== '[]') {
                var bloqueios = JSON.parse(jsonBloqueios);
                $('#bloqueiosContainer').empty();
                bloqueioIndex = 0;
                bloqueios.forEach(function (b) {
                    var html = criarBloqueioHtml(bloqueioIndex);
                    $('#bloqueiosContainer').append(html);
                    var $row = $('#bloqueiosContainer .bloco-bloqueio').last();
                    var de = b.De || b.de;
                    var ate = b.Ate || b.ate;
                    if (de && de.length >= 10) de = de.substring(0, 10);
                    if (ate && ate.length >= 10) ate = ate.substring(0, 10);
                    $row.find('.campo-bloq-de').val(de);
                    $row.find('.campo-bloq-ate').val(ate);
                    $row.find('.campo-bloq-motivo').val(b.Motivo || b.motivo);
                    bloqueioIndex++;
                });
            }

            // 4. Recria Meses (Visualmente)
            var strMeses = $('#<%= hdnMesesSelecionados.ClientID %>').val();
            if (strMeses) {
                var arr = strMeses.split(',');
                arr.forEach(function(chave) {
                    if(chave && chave.indexOf('-') > 0) {
                        var partes = chave.split('-');
                        var ano = parseInt(partes[0]);
                        var mes = parseInt(partes[1]);
                        // Data para Label
                        var d = new Date(ano, mes-1, 1);
                        var label = d.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
                        label = label.charAt(0).toUpperCase() + label.slice(1);

                        if(!mesesSelecionados.has(chave)) {
                            mesesSelecionados.set(chave, label);
                            criarCardMes(chave, label);
                        }
                    }
                });
                atualizarHiddenFieldMeses();
            }
        }

        // ============================================================
        // INICIALIZAÇÃO (DOMContentLoaded + jQuery)
        // ============================================================
        $(function () {
            // Popula o dropdown de meses ao iniciar
            preencherDropdownMeses();

            // Eventos de Botão
            $('#btnAddBloco').click(adicionarBloco);
            $('#btnAddBloqueio').click(adicionarBloqueio);

            // Evento Botão Adicionar Mês
            $('#btnAdicionarMes').click(function() {
                var ddl = document.getElementById('ddlMesesDisponiveis');
                var chave = ddl.value;
                if (!chave) { alert('Selecione um mês.'); return; }
                if (mesesSelecionados.has(chave)) { alert('Mês já adicionado.'); return; }
                
                var label = ddl.options[ddl.selectedIndex].text;
                mesesSelecionados.set(chave, label);
                criarCardMes(chave, label);
                atualizarHiddenFieldMeses();
            });

            // Evento Remover Mês (Delegado)
            $('#containerMesesSelecionados').on('click', '.btn-remover-mes', function() {
                var div = $(this).closest('[data-mes]');
                var chave = div.attr('data-mes');
                mesesSelecionados.delete(chave);
                div.remove();
                atualizarHiddenFieldMeses();
            });

            // Remove validação visual
            $('body').on('change input', '.is-invalid', function () { $(this).removeClass('is-invalid').closest('.erro-bloco').removeClass('erro-bloco'); });

            // Change Clínica
            $('#<%= ddlClinica.ClientID %>').change(function () {
                var cod = $(this).val();
                carregarSubespecialidades(cod);
                carregarProfissionaisPorClinica();
                if (cod) $(this).removeClass('is-invalid');
            });

            // Change Profissional
            $('#selProfissional').change(function () {
                var cod = $(this).val();
                var nome = $(this).find("option:selected").text();
                $('#<%= hdnCodProfissional.ClientID %>').val(cod);
                $('#<%= hdnNomeProfissional.ClientID %>').val(nome);
                if (cod) $(this).removeClass('is-invalid');
            });

            // MODO EDIÇÃO: Verifica se deve carregar dados
            var idEdicao = $('#<%= hdnIdPreAgendamento.ClientID %>').val();
            if (idEdicao) {
                carregarDadosEdicao();
            } else {
                adicionarBloco();
            }
        });
    </script>
</asp:Content>