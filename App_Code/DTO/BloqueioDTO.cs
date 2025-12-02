using System.Web.Script.Serialization;
using System.Collections.Generic;

public class BloqueioDTO
{
    public string De { get; set; }
    public string Ate { get; set; }
    public int CodMotivo { get; set; } // Agora é INT, pois o front envia o ID
    public string MotivoTexto { get; set; } // Opcional, apenas para display se necessário
}

