using System;
using System.Collections.Generic;
using System.Text;

namespace MS_Base.Models;

public class SistemaVersao
{
    public int Sistema_ID;         // ID do sistema do Config CORE
    public int Ambiente_ID;        // ID do ambiente do Config CORE
    public int RequestAmbiente_ID; // ID do ambiente do sistema Local
    public int RequestSistema_ID;  // ID do sistema Local
    public string vchVersao;
    public string vchIP;
}
