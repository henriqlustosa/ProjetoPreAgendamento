<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="visualizar_preagendamento.aspx.cs" Inherits="visualizar_preagendamento"
    Title="Pré-Agendamento - Aprovação" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        /* Estilos originais mantidos */
        body { background: #f8f9fa; font-family: 'Segoe UI', sans-serif; font-size: 1rem; }
        .preagendamento-container { padding: 20px 0 40px; }
        .page-title-card { border-radius: 8px 8px 0 0; background: #2A3F54; color: #fff; padding: 16px 22px; font-weight: 600; font-size: 1.1rem; }
        .card-block { background: #ffffff; border-radius: 0 0 8px 8px; box-shadow: 0 2px 6px rgba(0,0,0,0.08); padding: 22px 24px 12px; }
        .section-card { background: #ffffff; border-radius: 8px; box-shadow: 0 2px 6px rgba(0,0,0,0.04); padding: 18px 20px 12px; margin-bottom: 18px; }
        .section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; border-bottom: 1px solid #e2e6ea; padding-bottom: 6px; }
        .section-title { font-size: 1.15rem; font-weight: 600; color: #2A3F54; margin: 0; }
        .section-title i { margin-right: 6px; }
        .form-group label { font-size: .95rem; font-weight: 600; color: #495057; margin-bottom: 2px; }
        .form-control, select.form-control { border-radius: 6px; font-size: .95rem; padding: .45rem .6rem; }
        .day-box { border: 1px solid #dee2e6; border-radius: 8px; padding: 12px 12px 8px; margin-bottom: 10px; background: #fdfdff; }
        .day-title { font-weight: 600; margin-bottom: 10px; font-size: .95rem; color: #2A3F54; }
        .bloqueios-header { font-size: .95rem; font-weight: 600; margin-bottom: 6px; }
        
        /* --- NOVO CSS PARA MODO LEITURA --- */
        .ocultar-na-visualizacao { display: none !important; }
        
        /* Força aparência de desabilitado mesmo nos elementos JS */
        input[disabled], select[disabled], textarea[disabled] {
            background-color: #e9ecef !important;
            opacity: 1;
            cursor: not-allowed;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

    <div class="container-fluid preagendamento-container">
        <div class="row justify-content-center">
            <div class="col-xl-10 col-lg-11">

                <div class="page-title-card">
                    <asp:Label ID="lblTituloPagina" runat="server" Text="Visualizar e Aprovar Pré-Agendamento"></asp:Label>
                </div>

                <div class="card-block">
                    <asp:Panel ID="pnlLeitura" runat="server" Enabled="false">
                        
                        <div class="section-card">
                            <div class="section-header">
                                <div class="section-title"><i class="fas fa-user-md"></i>Dados Gerais</div>
                            </div>
                            <div class="form-row">
                                <div class="form-group col-md-4">
                                    <label>Data de Preenchimento</label>
                                    <asp:TextBox ID="txtDataPreenchimento" CssClass="form-control" runat="server" />
                                    <asp:HiddenField ID="hdnIdPreAgendamento" runat="server" />
                                </div>
                                <div class="form-group col-md-4">
                                    <label>Clínica</label>
                                    <asp:DropDownList ID="ddlClinica" CssClass="form-control" runat="server" />
                                </div>
                                <div class="form-group col-md-4">
                                    <label>Nome do Profissional</label>
                                    <select id="selProfissional" class="form-control" disabled="disabled">
                                        <option value="">Carregando...</option>
                                    </select>
                                    <asp:HiddenField ID="hdnCodProfissional" runat="server" />
                                    <asp:HiddenField ID="hdnNomeProfissional" runat="server" />
                                </div>
                            </div>
                        </div>

                        <div class="section-card">
                            <div class="section-header">
                                <div class="section-title"><i class="far fa-calendar-alt"></i>Meses Solicitados</div>
                                <button type="button" id="btnAdicionarMes" class="btn btn-outline-primary btn-sm ocultar-na-visualizacao">
                                    <i class="fas fa-plus-circle"></i> Adicionar mês
                                </button>
                            </div>
                             <div class="form-row mb-2 ocultar-na-visualizacao">
                                <div class="form-group col-md-4">
                                    <select id="ddlMesesDisponiveis" class="form-control"></select>
                                </div>
                            </div>
                            <div class="row" id="containerMesesSelecionados"></div>
                            <asp:HiddenField ID="hdnMesesSelecionados" runat="server" />
                        </div>

                        <div class="section-card">
                            <div class="section-header">
                                <div class="section-title"><i class="far fa-clock"></i>Horários por Dia da Semana</div>
                                <button type="button" id="btnAddBloco" class="btn btn-outline-primary btn-sm ocultar-na-visualizacao">
                                    <i class="fas fa-plus-circle"></i> Adicionar dia
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
                                <button type="button" id="btnAddBloqueio" class="btn btn-outline-secondary btn-sm ocultar-na-visualizacao">
                                    <i class="fas fa-plus-circle"></i> Adicionar bloqueio
                                </button>
                            </div>
                            <div id="bloqueiosContainer" class="mb-2"></div>
                            <asp:HiddenField ID="hdnBloqueiosJson" runat="server" />
                            <div class="form-group mt-3">
                                <label>Observações</label>
                                <asp:TextBox ID="txtObservacoes" CssClass="form-control" TextMode="MultiLine" Rows="4" runat="server" />
                            </div>
                        </div>

                    </asp:Panel>
                    <div class="actions-bar" style="border-top: 1px solid #dee2e6; padding-top: 20px; text-align: right;">
                        
                        <asp:Button ID="btnVoltar" CssClass="btn btn-default" Text="Voltar" OnClick="btnVoltar_Click" runat="server" style="margin-right: 10px;" />
                        
                        <asp:Button ID="btnAprovar" runat="server" Text="✔ APROVAR AGENDAMENTO" 
                            CssClass="btn btn-success" 
                            Style="background-color: #28a745; border-color: #28a745; color: white; font-weight: bold; padding: 10px 20px;"
                            OnClick="btnAprovar_Click" 
                            OnClientClick="return confirm('Tem certeza que deseja APROVAR este pré-agendamento? Ele sairá da fila de pendências.');" />

                    </div>
                </div>
            </div>
        </div>
    </div>

   <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

<script type="text/javascript">
    // --- VARIÁVEIS GLOBAIS ---
    var blocoIndex = 0;
    var bloqueioIndex = 0;
    var subespecialidadesCache = null;
    var motivosBloqueioCache = null;

    // --- FUNÇÕES GERADORAS DE HTML ---
    function criarBlocoHtml(index) {
        return '<div class="col-md-4 bloco-dia-wrapper" data-index="' + index + '">' +
            '  <div class="day-box bloco-dia">' +
            '    <div class="day-title">Dia da Semana</div>' +
            '    <label>Dia da Semana</label>' +
            '    <select class="form-control campo-dia-semana" disabled>' +
            '      <option value="">Selecione...</option>' +
            '      <option value="Segunda">Segunda</option>' +
            '      <option value="Terça">Terça</option>' +
            '      <option value="Quarta">Quarta</option>' +
            '      <option value="Quinta">Quinta</option>' +
            '      <option value="Sexta">Sexta</option>' +
            '    </select>' +
            '    <label class="mt-2">Horário</label>' +
            '    <input type="time" class="form-control campo-horario" disabled />' +
            '    <label class="mt-2">Consultas Novas</label>' +
            '    <input type="text" class="form-control campo-consultas-novas" disabled />' +
            '    <label class="mt-2">Consultas Retorno</label>' +
            '    <input type="text" class="form-control campo-consultas-retorno" disabled />' +
            '    <label class="mt-2">Subespecialidade</label>' +
            '    <select class="form-control campoSubespecialidade" disabled>' +
            '      <option value="">Carregando...</option>' +
            '    </select>' +
            '  </div>' +
            '</div>';
    }

    function criarBloqueioHtml(index) {
        return '<div class="row bloco-bloqueio mb-2" data-index="' + index + '">' +
            '  <div class="col-md-3"><label>De</label><input type="date" class="form-control campo-bloq-de" disabled /></div>' +
            '  <div class="col-md-3"><label>Até</label><input type="date" class="form-control campo-bloq-ate" disabled /></div>' +
            '  <div class="col-md-5"><label>Motivo</label><select class="form-control campo-bloq-motivo" disabled>' +
            '      <option value="">Selecione...</option>' +
            '  </select></div>' +
            '</div>';
    }

    // --- CARGAS AJAX ---
    function carregarSubespecialidades(codEspec) {
        if (!codEspec) return;
        PageMethods.ListarSubespecialidades(parseInt(codEspec), function (lista) {
            subespecialidadesCache = lista;
            $('.campoSubespecialidade').each(function () {
                var $ddl = $(this);
                var valorSalvo = $ddl.attr('data-selected');
                $ddl.empty();
                $.each(lista, function (i, item) {
                    $ddl.append($('<option>').val(item.CodSubespecialidade).text(item.NomeSubespecialidade));
                });
                if (valorSalvo) $ddl.val(valorSalvo);
            });
        }, function (e) { });
    }

    function carregarMotivosBloqueio(callback) {
        PageMethods.ListarMotivosBloqueio(
            function (result) {
                motivosBloqueioCache = result;
                if (callback) callback();
            },
            function (e) { console.log('Erro motivos: ' + e.get_message()); if (callback) callback(); }
        );
    }

    function preencherSelectMotivos($select) {
        $select.empty().append($('<option>').val('').text('Selecione...'));
        if (motivosBloqueioCache) {
            $.each(motivosBloqueioCache, function (i, item) {
                $select.append($('<option>').val(item.CodMotivo).text(item.NmMotivo));
            });
        }
    }

    // --- POPULAR TELA ---
    function carregarDadosVisualizacao() {

        // 1. PROFISSIONAL (Correção para garantir preenchimento)
        var nomeProf = $('#<%= hdnNomeProfissional.ClientID %>').val();
        var codProf = $('#<%= hdnCodProfissional.ClientID %>').val();

        // Verifica se tem nome, mesmo se o ID for string ou número
        if (nomeProf && nomeProf.trim() !== "") {
            $('#selProfissional').empty().append($('<option>', {
                value: codProf,
                text: nomeProf,
                selected: true
            }));
        } else {
            $('#selProfissional').empty().append($('<option>', { text: "Profissional não encontrado" }));
        }

        // 2. BLOCOS
        var jsonBlocos = $('#<%= hdnBlocosJson.ClientID %>').val();
        var temBlocos = false;

        if (jsonBlocos) {
            try {
                var blocos = JSON.parse(jsonBlocos);
                if (blocos.length > 0) {
                    temBlocos = true;
                    $('#blocosContainer').empty();
                    blocoIndex = 0;

                    // Tenta carregar subespecialidades da clínica atual
                    var codClinica = $('#<%= ddlClinica.ClientID %>').val();
                    if (codClinica) carregarSubespecialidades(codClinica);

                    blocos.forEach(function (b) {
                        var html = criarBlocoHtml(blocoIndex);
                        $('#blocosContainer').append(html);
                        var $row = $('#blocosContainer .bloco-dia-wrapper').last();

                        $row.find('.campo-dia-semana').val(b.DiaSemana || b.diaSemana);
                        $row.find('.campo-horario').val(b.Horario || b.horario);
                        $row.find('.campo-consultas-novas').val(b.ConsultasNovas || b.consultasNovas);
                        $row.find('.campo-consultas-retorno').val(b.ConsultasRetorno || b.consultasRetorno);

                        var codSub = b.CodSubespecialidade || b.codSubespecialidade;
                        var txtSub = b.SubespecialidadeTexto || b.subespecialidadeTexto;
                        var $ddlSub = $row.find('.campoSubespecialidade');

                        if (codSub) {
                            $ddlSub.attr('data-selected', codSub);
                            $ddlSub.empty().append(new Option(txtSub || "Carregando...", codSub, true, true));
                        }
                        blocoIndex++;
                    });
                }
            } catch (e) { console.error("Erro JSON blocos", e); }
        }
        
        // Mensagem caso não tenha blocos
        if (!temBlocos) {
            $('#blocosContainer').html('<div class="col-12"><div class="alert alert-warning">Nenhum horário cadastrado para este pré-agendamento.</div></div>');
        }

        // 3. BLOQUEIOS
        var jsonBloqueios = $('#<%= hdnBloqueiosJson.ClientID %>').val();
        if (jsonBloqueios) {
            try {
                var bloqueios = JSON.parse(jsonBloqueios);
                $('#bloqueiosContainer').empty();
                bloqueioIndex = 0;
                bloqueios.forEach(function (b) {
                    var html = criarBloqueioHtml(bloqueioIndex);
                    $('#bloqueiosContainer').append(html);
                    var $row = $('#bloqueiosContainer .bloco-bloqueio').last();

                    $row.find('.campo-bloq-de').val(b.De || b.de);
                    $row.find('.campo-bloq-ate').val(b.Ate || b.ate);
                    
                    var $ddlMotivo = $row.find('.campo-bloq-motivo');
                    preencherSelectMotivos($ddlMotivo);
                    $ddlMotivo.val(b.CodMotivo || b.codMotivo);

                    bloqueioIndex++;
                });
            } catch (e) { }
        }

        // 4. MESES (Já funcionava, mas garantindo a exibição)
        var strMeses = $('#<%= hdnMesesSelecionados.ClientID %>').val();
        if (strMeses) {
            $('#containerMesesSelecionados').empty(); // Limpa antes de popular
            var arr = strMeses.split(',');
            arr.forEach(function (chave) {
                if (chave && chave.indexOf('-') > 0) {
                    var partes = chave.split('-');
                    var d = new Date(parseInt(partes[0]), parseInt(partes[1]) - 1, 1);
                    var label = d.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
                    label = label.charAt(0).toUpperCase() + label.slice(1);

                    // Função auxiliar simples para criar card
                    var htmlCard = '<div class="col-md-3 col-sm-4 mb-3"><div class="card shadow-sm h-100"><div class="card-body"><div class="fw-bold">' + label + '</div><small class="text-muted">Solicitado</small></div></div></div>';
                    $('#containerMesesSelecionados').append(htmlCard);
                }
            });
        }
    }

    $(function () {
        // Inicia carregamento
        carregarMotivosBloqueio(function () {
            carregarDadosVisualizacao();
        });
    });
</script>
</asp:Content>

