using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Descrição resumida de BlocoDiaDTO
/// </summary>
public class BlocoDiaDTO
{
    public string DiaSemana { get; set; }
    public string Horario { get; set; }
    public string ConsultasNovas { get; set; }
    public string ConsultasRetorno { get; set; }

    // Usamos string ou int nullable para evitar erro se vier vazio ""
    public string CodSubespecialidade { get; set; }
    public string SubespecialidadeTexto { get; set; }

}
