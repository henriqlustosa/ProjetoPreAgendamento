using System;

public class PreAgendamentoDTO
{
    public int Id { get; set; }
    public DateTime DataPreenchimento { get; set; }

    public int CodEspecialidade { get; set; }
    public int CodProfissional { get; set; }

    public string Clinica { get; set; }         // opcional, texto
    public string Profissional { get; set; }    // opcional, texto

    public string Observacoes { get; set; }
    public string Usuario { get; set; }
    public DateTime DataCadastro { get; set; }
}

