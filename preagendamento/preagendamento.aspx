<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="preagendamento.aspx.cs" Inherits="publico_preagendamento"
    Title="Pré-Agendamento - Cadastro" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

    <style>
        .card-header {
            background: #004b8d;
            color: #fff;
            font-weight: bold;
        }

        .section-title {
            font-size: 1.15rem;
            font-weight: 600;
            margin-top: 25px;
            color: #004b8d;
        }

        .day-box {
            border: 1px solid #ccc;
            border-radius: 8px;
            padding: 10px;
            margin-bottom: 15px;
        }

        .day-title {
            font-weight: bold;
            margin-bottom: 10px;
        }
    </style>

</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

    <div class="container mt-4 mb-5">

        <div class="card shadow">
            <div class="card-header">
                Cadastro de Pré-Agendamento / Alteração de Agenda
            </div>

            <div class="card-body">

                <!-- 🔵 DADOS GERAIS -->
                <div class="section-title">Dados Gerais</div>

                <div class="form-row">
                    <div class="form-group col-md-3">
                        <label>Data de Preenchimento</label>
                        <asp:TextBox ID="txtDataPreenchimento"
                            CssClass="form-control"
                            TextMode="SingleLine"
                            runat="server" />
                    </div>

                    <div class="form-group col-md-4">
                        <label>Clínica</label>
                        <asp:DropDownList ID="ddlClinica" CssClass="form-control" runat="server" />
                    </div>

                    <div class="form-group col-md-5">
                        <label>Nome do Profissional</label>
                        <asp:TextBox ID="txtProfissional" CssClass="form-control" runat="server" />
                    </div>
                </div>

                <!-- 🔵 HORÁRIOS POR DIA DA SEMANA (BLOCOS DINÂMICOS) -->
                <div class="section-title">Horários por Dia da Semana</div>

                <div id="blocosContainer" class="row">
                    <!-- Blocos de dia serão inseridos aqui via JavaScript -->
                </div>

                <button type="button" id="btnAddBloco" class="btn btn-outline-primary btn-sm mt-2">
                    Adicionar Dia
                </button>

                <asp:HiddenField ID="hdnBlocosJson" runat="server" />

                <!-- 🔵 DADOS COMPLEMENTARES -->
                <div class="section-title">Dados Complementares</div>

                <label>Bloqueios</label>

                <div id="bloqueiosContainer" class="mb-2">
                    <!-- Linhas de bloqueio serão inseridas via JavaScript -->
                </div>

                <button type="button" id="btnAddBloqueio" class="btn btn-outline-secondary btn-sm mb-3">
                    Adicionar bloqueio
                </button>

                <asp:HiddenField ID="hdnBloqueiosJson" runat="server" />

                <!-- OBSERVAÇÕES -->
                <div class="form-group">
                    <label>Observações</label>
                    <asp:TextBox ID="txtObservacoes" CssClass="form-control" TextMode="MultiLine" Rows="4" runat="server" />
                </div>

                <!-- BOTÕES -->
                <div class="text-right mt-4">
                    <asp:Button ID="btnSalvar"
                        CssClass="btn btn-primary"
                        Text="Salvar"
                        OnClick="btnSalvar_Click"
                        OnClientClick="return PrepararBlocos();"
                        runat="server" />
                    <asp:Button ID="btnLimpar" CssClass="btn btn-secondary ml-2" Text="Limpar" runat="server" />
                </div>

            </div>
        </div>

    </div>

    <!-- 🔧 SCRIPT DOS BLOCOS DINÂMICOS + SUBESPECIALIDADE -->
    <script type="text/javascript">
        var blocoIndex = 0;
        var bloqueioIndex = 0;

        // cache das subespecialidades da clínica selecionada
        var subespecialidadesCache = null;

        // ========= BLOCOS DE DIA DA SEMANA =========
        function criarBlocoHtml(index) {
            var html = '';
            html += '<div class="col-md-4 bloco-dia-wrapper" data-index="' + index + '">';
            html += '  <div class="day-box bloco-dia">';
            html += '    <div class="day-title">Dia da Semana</div>';

            html += '    <label>Dia da Semana</label>';
            html += '    <select class="form-control campo-dia-semana">';
            html += '      <option value=\"\">Selecione...</option>';
            html += '      <option value=\"Segunda\">Segunda</option>';
            html += '      <option value=\"Terça\">Terça</option>';
            html += '      <option value=\"Quarta\">Quarta</option>';
            html += '      <option value=\"Quinta\">Quinta</option>';
            html += '      <option value=\"Sexta\">Sexta</option>';
            html += '    </select>';

            html += '    <label class="mt-2">Horário</label>';
            html += '    <input type="text" class="form-control campo-horario" placeholder="13:00" />';

            html += '    <label class="mt-2">Consultas Novas</label>';
            html += '    <input type="text" class="form-control campo-consultas-novas" />';

            html += '    <label class="mt-2">Consultas Retorno</label>';
            html += '    <input type="text" class="form-control campo-consultas-retorno" />';

            // 🔽 AGORA SUBESPECIALIDADE É UM SELECT
            html += '    <label class="mt-2">Subespecialidade</label>';
            html += '    <select class="form-control campoSubespecialidade">';
            html += '      <option value="">Selecione...</option>';
            html += '    </select>';

            html += '    <button type="button" class="btn btn-sm btn-outline-danger mt-2 btn-remover-bloco">Remover</button>';

            html += '  </div>';
            html += '</div>';

            return html;
        }

        function adicionarBloco() {
            var html = criarBlocoHtml(blocoIndex++);
            $('#blocosContainer').append(html);

            // se já tem subespecialidades carregadas, popula só o novo bloco
            if (subespecialidadesCache && subespecialidadesCache.length > 0) {
                var $novoSelect = $('#blocosContainer .bloco-dia-wrapper').last().find('.campoSubespecialidade');
                preencherSelectSubespecialidade($novoSelect, subespecialidadesCache);
            }
        }

        // ========= BLOQUEIOS (DE / ATÉ / MOTIVO) =========
        function criarBloqueioHtml(index) {
            var html = '';
            html += '<div class="row bloco-bloqueio mb-2" data-index="' + index + '">';

            html += '  <div class="col-md-3">';
            html += '    <label>De</label>';
            html += '    <input type="date" class="form-control campo-bloq-de" />';
            html += '  </div>';

            html += '  <div class="col-md-3">';
            html += '    <label>Até</label>';
            html += '    <input type="date" class="form-control campo-bloq-ate" />';
            html += '  </div>';

            html += '  <div class="col-md-5">';
            html += '    <label>Motivo</label>';
            html += '    <select class="form-control campo-bloq-motivo">';
            html += '        <option value="">Selecione...</option>';
            html += '        <option value="Férias">Férias</option>';
            html += '        <option value="Congresso">Congresso</option>';
            html += '        <option value="Ausência Justificada">Ausência Justificada</option>';
            html += '        <option value="Agenda Suspensa">Agenda Suspensa</option>';
            html += '        <option value="Bloqueio Administrativo">Bloqueio Administrativo</option>';
            html += '    </select>';
            html += '  </div>';

            html += '  <div class="col-md-1 d-flex align-items-end">';
            html += '    <button type="button" class="btn btn-sm btn-outline-danger btn-remover-bloqueio">X</button>';
            html += '  </div>';

            html += '</div>';

            return html;
        }

        function adicionarBloqueio() {
            var html = criarBloqueioHtml(bloqueioIndex++);
            $('#bloqueiosContainer').append(html);
        }

        // ========= MONTAR JSON ANTES DO POSTBACK =========
        function PrepararBlocos() {
            var blocos = [];
            var bloqueios = [];

            // Dias da semana
            $('#blocosContainer .bloco-dia').each(function () {
                var $b = $(this);
                var $ddlSub = $b.find('.campoSubespecialidade');

                var obj = {
                    diaSemana: $b.find('.campo-dia-semana').val(),
                    horario: $b.find('.campo-horario').val(),
                    consultasNovas: $b.find('.campo-consultas-novas').val(),
                    consultasRetorno: $b.find('.campo-consultas-retorno').val(),
                    // mantém o nome da subespecialidade como antes
                    subespecialidade: $ddlSub.find('option:selected').text(),
                    // opcional: já manda também o ID (se depois você criar a propriedade no DTO)
                    subespecialidadeId: $ddlSub.val()
                };

                if (obj.diaSemana || obj.horario || obj.consultasNovas || obj.consultasRetorno || obj.subespecialidade) {
                    blocos.push(obj);
                }
            });

            // Bloqueios
            $('#bloqueiosContainer .bloco-bloqueio').each(function () {
                var $b = $(this);
                var b = {
                    de: $b.find('.campo-bloq-de').val(),
                    ate: $b.find('.campo-bloq-ate').val(),
                    motivo: $b.find('.campo-bloq-motivo').val()
                };

                if (b.de || b.ate || b.motivo) {
                    bloqueios.push(b);
                }
            });

            // grava nos hidden fields
            $('#<%= hdnBlocosJson.ClientID %>').val(JSON.stringify(blocos));
            $('#<%= hdnBloqueiosJson.ClientID %>').val(JSON.stringify(bloqueios));

            return true; // permite o postback
        }

        // ========= SUBESPECIALIDADES (AJAX) =========
        function preencherSelectSubespecialidade($ddl, lista) {
            $ddl.empty();
            $ddl.append($('<option>').val('').text('Selecione...'));
            $.each(lista, function (i, item) {
                $ddl.append(
                    $('<option>').val(item.CodSubespecialidade).text(item.NomeSubespecialidade)
                );
            });
        }

        function carregarSubespecialidades(codEspec) {
            if (!codEspec) {
                // Limpa todas as combos
                $('.campoSubespecialidade').each(function () {
                    $(this).empty().append($('<option>').val('').text('Selecione...'));
                });
                subespecialidadesCache = null;
                return;
            }

            PageMethods.ListarSubespecialidades(
                parseInt(codEspec),
                function (lista) {
                    subespecialidadesCache = lista;
                    $('.campoSubespecialidade').each(function () {
                        preencherSelectSubespecialidade($(this), lista);
                    });
                },
                function (err) {
                    console.log(err);
                    alert('Erro ao carregar subespecialidades.');
                }
            );
        }

        // ========= INICIALIZAÇÃO =========
        $(function () {
            // bloco de dia inicial
            adicionarBloco();

            $('#btnAddBloco').click(function () {
                adicionarBloco();
            });

            $('#blocosContainer').on('click', '.btn-remover-bloco', function () {
                $(this).closest('.bloco-dia-wrapper').remove();
            });

            // bloqueio inicial
            adicionarBloqueio();

            $('#btnAddBloqueio').click(function () {
                adicionarBloqueio();
            });

            $('#bloqueiosContainer').on('click', '.btn-remover-bloqueio', function () {
                $(this).closest('.bloco-bloqueio').remove();
            });

            // quando mudar a clínica, carrega subespecialidades
            $('#<%= ddlClinica.ClientID %>').change(function () {
                var codEspec = $(this).val();
                carregarSubespecialidades(codEspec);
            });
        });
    </script>
</asp:Content>
