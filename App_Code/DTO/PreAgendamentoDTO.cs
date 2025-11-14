using System;

public class PreAgendamentoDTO
{
    public int Id { get; set; }
    public DateTime DataPreenchimento { get; set; }
    public string Clinica { get; set; }
    public string Profissional { get; set; }


    public string Observacoes { get; set; }

    public string Usuario { get; set; }
    public DateTime DataCadastro { get; set; }
}

