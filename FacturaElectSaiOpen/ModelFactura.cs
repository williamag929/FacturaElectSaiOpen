using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturaElectSaiOpen
{

    #region modelos

    public class config
    {
        public string ruta_emision { get; set; }
        public string ruta_adjuntos { get; set; }
        public string tokenempresa { get; set; }
        public string tokenpassword { get; set; }

    }

    public class respuestaElectronica
    {
        public int codigo { get; set; }
        public string consecutivo { get; set; }
        public string cufe { get; set; }
        public string mensaje { get; set; }
        public string resultado { get; set; }
        public string[] mensajevalidacion { get; set; }
    }

    public class FacturaEnc
    {
        public string tipodoc { get; set; }
        public string prefjo { get; set; }
        public int numero { get; set; }
        public int resnumdesde { get; set; }
        public int resmunhasta { get; set; }
        public DateTime fecha { get; set; }
        public DateTime fechavence { get; set; }
        public string nit { get; set; }
        public string dv { get; set; }
        public string company { get; set; }
        public string direccion { get; set; }
        public string telefono1 { get; set; }
        public string codciudad { get; set; }
        public string ciudad { get; set; }
        public string coddepto { get; set; }
        public string departamento { get; set; }
        public string correoe { get; set; }
        public string numordencompra { get; set; }
        public string numremision { get; set; }
        public string comentario { get; set; }
        public string tipoIdentificacion { get; set; }
        public string tipoPersona { get; set; }
        public decimal vlrbruto { get; set; }
        public decimal subtotal { get; set; }
        public decimal impuesto { get; set; }
        public decimal total { get; set; }
        public decimal baseretencion { get; set; }
        public decimal porcretefuente { get; set; }
        public decimal retefuente { get; set; }
        public decimal porcreteica { get; set; }
        public decimal reteiva { get; set; }
        public decimal porcreteiva { get; set; }
        public decimal reteica { get; set; }
        public decimal descuentos { get; set; }
        public string codigociiu { get; set; }
        public string crucetipo { get; set; }
        public string crucenumero { get; set; }
        public string cufe { get; set; }

    }

    public class facturadet
    {
        public string codproducto { get; set; }
        public string referencia { get; set; }
        public string descprocucto { get; set; }
        public string descalterna { get; set; }
        public decimal cantidad { get; set; }
        public decimal precio { get; set; }
        public decimal porcimp { get; set; }
        public decimal impuesto { get; set; }
        public decimal descuentol { get; set; }
        public decimal descuentovalor { get; set; }
        public decimal subtotal { get; set; }
        public decimal total { get; set; }
    }

    public class impuestos
    {
        public string codigo { get; set; }
        public decimal porcimp { get; set; }
        public decimal subtotal { get; set; }
        public decimal valor { get; set; }
    }

    public class formapago
    {
        public string concepto { get; set; }
        public string tipo { get; set; }
        public decimal valor { get; set; }
        public int dias { get; set; }
        public DateTime fecha { get; set; }

    }
    #endregion


    #region metodos



    #endregion

}
