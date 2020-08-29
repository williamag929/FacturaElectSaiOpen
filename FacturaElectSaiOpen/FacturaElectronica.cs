using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Net;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Threading.Tasks;
using FacturaElectSaiOpen.tfhk_Emision;
using System.Configuration;

namespace FacturaElectSaiOpen
{
    class FacturaElectronica
    {


        #region v2 HKA


        public string tokenEmpresa = "ea6f416e94b54d8d82c53d04f7d55b1470bde0f3"; // los accesos deben ser solicitados al iniciar el proceso de integración
        public string tokenAuthorizacion = "1c56afa3a0544f50ae66df477598bed59f48cfb6"; // los accesos deben ser solicitados al iniciar el proceso de integración
        public string Url = "";





        //private serpi_Entities context = new serpi_Entities();


        tfhk_Emision.ServiceClient serviceClient;
        tfhk_Adjuntos.ServiceClient serviceClientAdjuntos;


        tfhk_Adjuntos.ServiceClient serviceArchivos;

        config cnfg = new config();


        //    serviceArchivos = new ServiceAdjuntos.ServiceClient();



        //#region Constructor de la clase
        //public Form1()
        //{
        //    InitializeComponent();
        //    serviceClient = new ServiceEmision.ServiceClient();
        //    serviceArchivos = new ServiceAdjuntos.ServiceClient();
        //}
        //#endregion

        public class _detimpuesto
        {
            public decimal? porcentaje { get; set; }
            public decimal valoriva { get; set; }
            public decimal valorbase { get; set; }
        }

        public class _resimpuesto
        {
            public string codigo { get; set; }
            public decimal valor { get; set; }
        }

        public class unidadmedida
        {
            public string codigo { get; set; }
            public string descripcion { get; set; }
            public string equivalente { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlemision"></param>
        /// <param name="urlAdjuntos"></param>
        public void inicializar(string urlemision, string urlAdjuntos)
        {
            //serviceClient = new tfhk_Emision.ServiceClient();
            //serviceArchivos = new tfhk_Adjuntos.ServiceClient();

            BasicHttpBinding port = new BasicHttpBinding();
            port.MaxBufferPoolSize = Int32.MaxValue;
            port.MaxBufferSize = Int32.MaxValue;
            port.MaxReceivedMessageSize = Int32.MaxValue;
            port.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            port.SendTimeout = TimeSpan.FromMinutes(2);
            port.ReceiveTimeout = TimeSpan.FromMinutes(2);
            port.Security.Mode = BasicHttpSecurityMode.Transport;
            //Especifica la dirección de conexion para Demo y Adjuntos
            EndpointAddress endPointEmision = new EndpointAddress(urlemision); //URL DEMO EMISION http://demoemision21.thefactoryhka.com.co/ws/v1.0/Service.svc?wsdl
            EndpointAddress endPointAdjuntos = new EndpointAddress(urlAdjuntos); //URL DEMO ADJUNTOS http://demoemision21.thefactoryhka.com.co/ws/adjuntos/Service.svc?wsdl       
            //Se instancia el cliente para los servicios Demo y Adjuntos
            serviceClient = new tfhk_Emision.ServiceClient(port, endPointEmision);
            serviceClientAdjuntos = new tfhk_Adjuntos.ServiceClient(port, endPointAdjuntos);
        }

        #region Construcción del Objeto Factura
        private FacturaGeneral BuildFactura(string tipodoc, int numero)
        {
            var f = new FacturaEnc();

            //cargar datos factura

            var fc = new facturaController();


            f = fc.getfacturaenc(tipodoc, numero);


            List<unidadmedida> unidades = new List<unidadmedida>();

            unidades.Add(new unidadmedida { codigo = "94", descripcion = "Unidad", equivalente = "und" });
            unidades.Add(new unidadmedida { codigo = "LBR", descripcion = "Libra", equivalente = "lb" });
            unidades.Add(new unidadmedida { codigo = "LBR", descripcion = "Libra", equivalente = "lbr" });
            unidades.Add(new unidadmedida { codigo = "PK", descripcion = "Paquete", equivalente = "paq" });
            unidades.Add(new unidadmedida { codigo = "PA", descripcion = "Paquete", equivalente = "pa" });
            unidades.Add(new unidadmedida { codigo = "MTR", descripcion = "Metro", equivalente = "mtr" });
            unidades.Add(new unidadmedida { codigo = "CGR", descripcion = "Gramo", equivalente = "gr" });
            unidades.Add(new unidadmedida { codigo = "KGM", descripcion = "Kilogramo", equivalente = "kgr" });
            unidades.Add(new unidadmedida { codigo = "KGM", descripcion = "Kilogramo", equivalente = "kg" });

            //validar que el tercero tenga correo
            //validar que el tercero tenga codigo CIIU

            //validar item bonificado
            //var itmbon = f.gdtventadet.Where(c => c.esbonificado == true).Count();

            //validar AIU
            //var itmaiu = f.gdtventadet.Where(c => c.aplicaaiu == true).Count();

            //armo el objeto factura
            FacturaGeneral factura = new FacturaGeneral();



            #region anticipos
            factura.anticipos = null;
            #endregion

            #region autorizado
            factura.autorizado = null;
            #endregion

            var decimales = 2;

            factura.cantidadDecimales = decimales.ToString();

            #region cargosDescuentos
            factura.cargosDescuentos = null;
            #endregion

            #region cliente
            Cliente cliente = new Cliente();

            cliente.actividadEconomicaCIIU = f.codigociiu;
            cliente.apellido = null;

            cliente.destinatario = new Destinatario[1];
            Destinatario destinatario1 = new Destinatario();
            destinatario1.canalDeEntrega = "0";
            string[] correoEntrega = new string[1];
            correoEntrega[0] = f.correoe;
            destinatario1.email = correoEntrega;
            destinatario1.fechaProgramada = f.fecha.ToString("yyyy-MM-dd 00:00:00");
            destinatario1.mensajePersonalizado = null;
            destinatario1.nitProveedorReceptor = f.nit;
            destinatario1.telefono = f.telefono1;

            cliente.destinatario[0] = destinatario1;


            //tributos impuesto cliente gravable
            cliente.detallesTributarios = new Tributos[1];
            Tributos tributos1 = new Tributos();
            tributos1.codigoImpuesto = "01";
            tributos1.extras = null;
            cliente.detallesTributarios[0] = tributos1;
            //cliente.detallesTributarios = null;

            Direccion direccionFiscal = new Direccion();
            direccionFiscal.aCuidadoDe = null;
            direccionFiscal.aLaAtencionDe = null;
            direccionFiscal.bloque = null;
            direccionFiscal.buzon = null;
            direccionFiscal.calle = null;
            direccionFiscal.calleAdicional = null;
            direccionFiscal.ciudad = f.ciudad;
            direccionFiscal.codigoDepartamento = f.coddepto;
            direccionFiscal.correccionHusoHorario = null;
            direccionFiscal.departamento = f.departamento;
            direccionFiscal.departamentoOrg = null;
            direccionFiscal.direccion = f.direccion;
            direccionFiscal.distrito = null;
            direccionFiscal.habitacion = null;
            direccionFiscal.lenguaje = "es";
            direccionFiscal.localizacion = null;
            direccionFiscal.municipio = f.codciudad;
            direccionFiscal.nombreEdificio = null;
            direccionFiscal.numeroEdificio = null;
            direccionFiscal.numeroParcela = null;
            direccionFiscal.pais = "CO";//f.gdttercero.gdtpais.nombre.Substring(1, 2);
            direccionFiscal.piso = null;
            direccionFiscal.region = null;
            direccionFiscal.subDivision = null;
            direccionFiscal.ubicacion = null;
            direccionFiscal.zonaPostal = null;

            //se deshabilito por problema con terceros del exterior Enero 2020
            //direccionFiscal.zonaPostal = f.gdttercero.gdtciudad.codpostal.ToString();
            cliente.direccionFiscal = direccionFiscal;
            //cliente.direccionFiscal = null;

            cliente.email = f.correoe;
            cliente.extras = null;

            InformacionLegal informacionLegalCliente = new InformacionLegal();
            informacionLegalCliente.codigoEstablecimiento = "00001";
            informacionLegalCliente.nombreRegistroRUT = f.company;
            informacionLegalCliente.numeroIdentificacion = f.nit;
            informacionLegalCliente.numeroIdentificacionDV = f.dv;
            informacionLegalCliente.numeroMatriculaMercantil = null;
            informacionLegalCliente.prefijoFacturacion = null;
            informacionLegalCliente.tipoIdentificacion = f.tipoIdentificacion;
            cliente.informacionLegalCliente = informacionLegalCliente;
            cliente.direccionCliente = direccionFiscal;

            cliente.nombreComercial = null;
            cliente.nombreContacto = null;
            cliente.nombreRazonSocial = f.company;
            cliente.nota = null;
            cliente.notificar = "SI";
            cliente.numeroDocumento = f.nit;
            cliente.numeroIdentificacionDV = f.dv.ToString();



            //obligatorio?

            cliente.responsabilidadesRut = new Obligaciones[1];
            Obligaciones obligaciones1 = new Obligaciones();
            obligaciones1.obligaciones = "R-99-PN";
            obligaciones1.regimen = "04";  //4 Régimen Simple  //5 Régimen Ordinario
            obligaciones1.extras = null;
            cliente.responsabilidadesRut[0] = obligaciones1;


            cliente.segundoNombre = null;
            cliente.telefax = null;
            cliente.telefono = f.telefono1;
            cliente.tipoIdentificacion = f.tipoIdentificacion;
            cliente.tipoPersona = f.tipoPersona;

            factura.cliente = cliente;
            #endregion 



            #region condicionPago
            factura.condicionPago = null;
            #endregion


            factura.consecutivoDocumento = f.numero.ToString();

            #region detalleDeFactura

            //cargar detalles

            var detalles = new List<facturadet>();

            detalles = fc.getfacturadet(tipodoc, numero);

            factura.detalleDeFactura = new FacturaDetalle[detalles.Count()];

            decimal totbasimpMuestraG = 0;

            var i = 0;
            foreach (var item in detalles)
            {


                FacturaDetalle producto1 = new FacturaDetalle();
                producto1.cantidadPorEmpaque = "1";
                producto1.cantidadReal = Math.Round(item.cantidad, decimales).ToString();
                //producto1.cantidadRealUnidadMedida = "WSD";

                var existeund = unidades.Where(c => c.codigo == item.unidadmedida);

                producto1.cantidadRealUnidadMedida = item.unidadmedida;

                if (existeund.Count() == 0)
                {
                    producto1.cantidadRealUnidadMedida = "WSD";
                }

                

                producto1.cantidadUnidades = Math.Round(item.cantidad, decimales).ToString();
                producto1.cargosDescuentos = null;
                if (item.descuentovalor > 0)
                {
                    var cargosDescuentos = new CargosDescuentos[1];
                    CargosDescuentos desc1 = new CargosDescuentos();
                    desc1.indicador = "0";
                    desc1.monto = Math.Round(item.descuentovalor, decimales).ToString();
                    desc1.montoBase = Math.Round(item.cantidad * item.precio, decimales).ToString();
                    desc1.porcentaje = Math.Round(item.descuentol, decimales).ToString();
                    desc1.secuencia = "1";
                    desc1.descripcion = "Descuento Comercial";
                    cargosDescuentos[0] = desc1;

                    producto1.cargosDescuentos = cargosDescuentos;
                }

                producto1.codigoFabricante = null;
                producto1.codigoIdentificadorPais = null;
                producto1.codigoProducto = item.codproducto;
                producto1.codigoTipoPrecio = null;
                producto1.descripcion = item.descprocucto;
                producto1.descripcionTecnica = item.descalterna;
                producto1.documentosReferenciados = null;
                producto1.estandarCodigo = item.referencia;
                producto1.estandarCodigoID = null;
                producto1.estandarCodigoIdentificador = null;
                producto1.estandarCodigoNombre = null;
                producto1.estandarCodigoProducto = item.codproducto;
                producto1.estandarOrganizacion = null;
                producto1.estandarSubCodigoProducto = null;
                //producto1.nota = item.comentario;
                producto1.nota = null;


                producto1.impuestosDetalles = new FacturaImpuestos[1];
                FacturaImpuestos impuesto1 = new FacturaImpuestos();

                impuesto1.baseImponibleTOTALImp = Math.Round(item.subtotal, decimales).ToString();  //cambiar para AIU
                //if (item.esbonificado == true)
                //{
                //    impuesto1.baseImponibleTOTALImp = Math.Round(item.basedet.Value * item.cantidad, decimales).ToString();  //cambiar para AIU
                //    totbasimpMuestraG = totbasimpMuestraG + (item.basedet.Value * item.cantidad);
                //}



                impuesto1.codigoTOTALImp = "01";  //IVA
                impuesto1.controlInterno = "";
                if (item.impuesto > 0)
                {

                    impuesto1.porcentajeTOTALImp = Math.Round(item.porcimp, 2).ToString();
                }
                else
                {
                    item.porcimp = 0;
                    impuesto1.porcentajeTOTALImp = Math.Round(0.00, 2).ToString();
                }
                impuesto1.unidadMedida = "";
                impuesto1.unidadMedidaTributo = "";
                impuesto1.valorTOTALImp = Math.Round(item.impuesto, decimales).ToString();
                impuesto1.valorTributoUnidad = "";
                producto1.impuestosDetalles[0] = impuesto1;

                producto1.impuestosTotales = new ImpuestosTotales[1];
                ImpuestosTotales impuestoTOTAL1 = new ImpuestosTotales();
                impuestoTOTAL1.codigoTOTALImp = "01";  //IVA 
                impuestoTOTAL1.montoTotal = Math.Round(item.impuesto, decimales).ToString();
                producto1.impuestosTotales[0] = impuestoTOTAL1;


                //                LineaInformacionAdicional infoadic = new LineaInformacionAdicional();




                producto1.informacionAdicional = null;



                producto1.mandatorioNumeroIdentificacion = null;
                producto1.mandatorioNumeroIdentificacionDV = null;
                producto1.mandatorioTipoIdentificacion = null;
                producto1.marca = "N/A";
                producto1.modelo = null;

                producto1.muestraGratis = "0";  //item bonificado
                producto1.precioTotal = Math.Round(item.subtotal + item.impuesto, decimales).ToString();
                producto1.precioTotalSinImpuestos = Math.Round(item.subtotal, decimales).ToString();
                producto1.precioVentaUnitario = Math.Round(item.precio, decimales).ToString();
                //producto1.precioReferencia = "0.00";


                //if (item.esbonificado == true)
                //{
                //    producto1.muestraGratis = "1";
                //    producto1.precioReferencia = Math.Round(item.basedet.Value, decimales).ToString();
                //    producto1.precioTotal = "0.00";
                //    producto1.precioTotalSinImpuestos = "0.00";
                //    producto1.precioVentaUnitario = "0.00";
                //    producto1.codigoTipoPrecio = "01";

                //}

                producto1.nombreFabricante = null;
                //producto1.nota = null;


                producto1.secuencia = (i+1).ToString();
                producto1.seriales = null;
                producto1.subCodigoFabricante = null;
                producto1.subCodigoProducto = null;
                producto1.tipoAIU = null;
                producto1.unidadMedida = item.unidadmedida;






                factura.detalleDeFactura[i] = producto1;

                i++;

            }
            #endregion

            #region documentosReferenciados

            //si el documento es la factura envia null
            if (f.prefijo == ConfigurationManager.AppSettings.Get("prefijofe"))
            {

                factura.documentosReferenciados = null;
            }
            else
            {
                try
                {



                    factura.documentosReferenciados = new DocumentoReferenciado[2];

                    string clase = fc.GetSiglafactura(f.crucetipo);

                    f.crucetipo = clase;

                    var cufeorigen = fc.getfacturaenc(f.crucetipo, Convert.ToInt32(f.crucenumero)).cufe;

                    DocumentoReferenciado documento1 = new DocumentoReferenciado();
                    documento1.codigoEstatusDocumento = "2";
                    documento1.codigoInterno = "4";
                    documento1.cufeDocReferenciado = cufeorigen.Trim();
                    var motivo = new string[1];
                    motivo[0] = "Referencia: " + f.numero;
                    documento1.descripcion = motivo;
                    documento1.numeroDocumento = f.crucetipo.Trim() + f.crucenumero.ToString().Trim();
                    factura.documentosReferenciados[0] = documento1;

                    DocumentoReferenciado documento2 = new DocumentoReferenciado();

                    documento2.codigoInterno = "5";
                    documento2.cufeDocReferenciado = cufeorigen.Trim();
                    documento2.fecha = f.fecha.ToString("yyyy-MM-dd");
                    documento2.numeroDocumento = f.crucetipo.Trim() + f.crucenumero.ToString().Trim();
                    factura.documentosReferenciados[1] = documento2;
                    //numcuota++;
                }
                catch { }

            }

            #endregion

            #region entregaMercancia
            factura.entregaMercancia = null;
            #endregion

            #region extras

            //< Extras >
            //    < controlInterno1 > Total Base Excluida</ controlInterno1 >
            //       < controlInterno2 ></ controlInterno2 >
            //       < nombre > 5170000 </ nombre >
            //       < pdf > 1 </ pdf >
            //       < valor > 10000.25 </ valor >
            //       < xml > 1 </ xml >
            //   </ Extras >

            //factura.extras = new Extras[2];
            //Extras extra0 = new Extras();
            //extra0.controlInterno1 = "Orden Compra";
            //extra0.controlInterno2 = null;
            //extra0.nombre = "5170000";
            //extra0.pdf = "1";
            //extra0.valor = f.numordencompra;
            //extra0.xml = "1";






            //producto1 = new factruae();

            #endregion




            factura.fechaEmision = DateTime.Now.ToString("yyyy-MM-dd 00:00:00"); ;
            factura.fechaFinPeriodoFacturacion = null;
            factura.fechaInicioPeriodoFacturacion = null;
            factura.fechaPagoImpuestos = null;
            factura.fechaVencimiento = null;

            #region impuestosGenerales

            //todo: agrupar impuestos

            List<_detimpuesto> _impuesto = new List<_detimpuesto>();
            var dg = detalles.GroupBy(x => new
            {
                x.porcimp,
            }).Select(g => new _detimpuesto()
            {
                porcentaje = g.Key.porcimp,
                valoriva = g.Sum(c => c.impuesto),
                valorbase = g.Sum(c => c.subtotal)
            });

            var imp = 0;
            var countimp = dg.Where(c => c.porcentaje > 0).Count();

            var countret = 0;

            foreach (var item in dg.Where(c => c.porcentaje > 0))
            {
                countret++;
            }

            if (f.retefuente > 0)
            {
                countret++;
            }
            if (f.reteica > 0)
            {
                countret++;
            }
            if (f.reteiva > 0)
            {
                countret++;
            }


            //impuesto iva general
            //este quedo ok vamos a consolidar

            List<_resimpuesto> _impgeneral = new List<_resimpuesto>();

            decimal totimpuesto = 0;

            factura.impuestosGenerales = new FacturaImpuestos[countret];
            foreach (var item in dg.Where(c => c.porcentaje > 0))
            {

                FacturaImpuestos impuestoGeneral1 = new FacturaImpuestos();
                impuestoGeneral1.baseImponibleTOTALImp = Math.Round(item.valorbase + totbasimpMuestraG, decimales).ToString();  //verificar con AIU  
                impuestoGeneral1.codigoTOTALImp = "01";
                impuestoGeneral1.controlInterno = null;
                impuestoGeneral1.porcentajeTOTALImp = Math.Round(item.porcentaje.Value, 2).ToString();
                impuestoGeneral1.unidadMedida = "WSD";
                impuestoGeneral1.unidadMedidaTributo = null;
                impuestoGeneral1.valorTOTALImp = Math.Round(item.valoriva, decimales).ToString();
                impuestoGeneral1.valorTributoUnidad = null;
                factura.impuestosGenerales[imp] = impuestoGeneral1;
                imp++;
                totimpuesto = totimpuesto + item.valoriva;
            }

            if (totimpuesto > 0)
            {
                _impgeneral.Add(new _resimpuesto { codigo = "01", valor = totimpuesto });
            }


            //retenciones generales retefuente/reteica/reteiva
            if (countret > 0)
            {
                //impuestos ICA RETEN ...
                if (f.retefuente > 0)
                {
                    FacturaImpuestos impuestoGeneral2 = new FacturaImpuestos();
                    impuestoGeneral2.baseImponibleTOTALImp = Math.Round(f.baseretencion, 2).ToString();
                    impuestoGeneral2.codigoTOTALImp = "06";
                    f.porcretefuente = f.retefuente / f.baseretencion * 100;
                    impuestoGeneral2.porcentajeTOTALImp = Math.Round(f.porcretefuente, 2).ToString();
                    impuestoGeneral2.unidadMedida = "WSD";
                    impuestoGeneral2.valorTOTALImp = Math.Round(f.retefuente, decimales).ToString();
                    factura.impuestosGenerales[imp] = impuestoGeneral2;
                    imp++;

                    _impgeneral.Add(new _resimpuesto { codigo = "06", valor = f.retefuente });

                }

                if (f.reteica > 0)
                {
                    FacturaImpuestos impuestoGeneral = new FacturaImpuestos();
                    impuestoGeneral.baseImponibleTOTALImp = Math.Round(f.baseretencion, 2).ToString();
                    impuestoGeneral.codigoTOTALImp = "07";
                    f.porcreteica = f.reteica / f.baseretencion * 100;
                    impuestoGeneral.porcentajeTOTALImp = Math.Round(f.porcreteica, 2).ToString();
                    impuestoGeneral.unidadMedida = "WSD";
                    impuestoGeneral.valorTOTALImp = Math.Round(f.reteica, decimales).ToString();
                    factura.impuestosGenerales[imp] = impuestoGeneral;
                    imp++;

                    _impgeneral.Add(new _resimpuesto { codigo = "07", valor = f.reteica });

                }

                if (f.reteiva > 0)
                {
                    FacturaImpuestos impuestoGeneral = new FacturaImpuestos();
                    impuestoGeneral.baseImponibleTOTALImp = Math.Round(f.impuesto, 2).ToString();
                    impuestoGeneral.codigoTOTALImp = "05";
                    f.porcreteiva = f.reteiva / f.impuesto * 100;
                    impuestoGeneral.porcentajeTOTALImp = Math.Round(f.porcreteiva, 2).ToString();
                    impuestoGeneral.unidadMedida = "WSD";
                    impuestoGeneral.valorTOTALImp = Math.Round(f.reteiva, decimales).ToString();
                    factura.impuestosGenerales[imp] = impuestoGeneral;
                    imp++;

                    _impgeneral.Add(new _resimpuesto { codigo = "05", valor = f.reteiva });

                }
            }

            #endregion

            //este fallo se cambia por consolidado
            #region impuestosTotales

            if (_impgeneral.Count() > 0)
            {
                factura.impuestosTotales = new ImpuestosTotales[_impgeneral.Count()];


                //estas lineas remplaza las lineas del 637 al 684
                var impgen = 0;
                foreach(var item in _impgeneral)
                {
                    ImpuestosTotales impuestoGeneralTOTAL0 = new ImpuestosTotales();
                    impuestoGeneralTOTAL0.codigoTOTALImp = item.codigo;
                    impuestoGeneralTOTAL0.montoTotal = Math.Round(item.valor, decimales).ToString();
                    factura.impuestosTotales[impgen] = impuestoGeneralTOTAL0;
                    impgen++;
                }

                ////IVA
                //var reten = 0;
                ////foreach (var item in dg.Where(c => c.porcentaje > 0))
                ////if (dg.Where(c => c.porcentaje > 0).Count() > 0)
                //foreach (var item in dg.Where(c => c.porcentaje > 0))
                //{
                //    ImpuestosTotales impuestoGeneralTOTAL0 = new ImpuestosTotales();
                //    impuestoGeneralTOTAL0.codigoTOTALImp = "01";
                //    impuestoGeneralTOTAL0.montoTotal = Math.Round(item.valoriva, decimales).ToString();
                //    factura.impuestosTotales[reten] = impuestoGeneralTOTAL0;
                //    reten++;
                //}
                ////IVA

                ////foreach (var item in dg.Where(c => c.porcentaje > 0))

                ////ImpuestosTotales impuestoGeneralTOTAL0 = new ImpuestosTotales();
                ////impuestoGeneralTOTAL0.codigoTOTALImp = "01";
                ////impuestoGeneralTOTAL0.montoTotal = Math.Round(dg.Sum(x => x.valoriva), decimales).ToString();
                ////factura.impuestosTotales[0] = impuestoGeneralTOTAL0;


                ////impuestos ICA RETEN ...
                //if (f.retefuente > 0)
                //{
                //    ImpuestosTotales impuestoGeneralTOTAL1 = new ImpuestosTotales();
                //    impuestoGeneralTOTAL1.codigoTOTALImp = "06";
                //    impuestoGeneralTOTAL1.montoTotal = Math.Round(f.retefuente, decimales).ToString();
                //    factura.impuestosTotales[reten] = impuestoGeneralTOTAL1;
                //    reten++;
                //}

                //if (f.reteica > 0)
                //{
                //    ImpuestosTotales impuestoGeneralTOTAL2 = new ImpuestosTotales();
                //    impuestoGeneralTOTAL2.codigoTOTALImp = "07";
                //    impuestoGeneralTOTAL2.montoTotal = Math.Round(f.reteica, decimales).ToString();
                //    factura.impuestosTotales[reten] = impuestoGeneralTOTAL2;
                //    reten++;
                //}

                //if (f.reteiva > 0)
                //{
                //    ImpuestosTotales impuestoGeneralTOTAL3 = new ImpuestosTotales();
                //    impuestoGeneralTOTAL3.codigoTOTALImp = "05";
                //    impuestoGeneralTOTAL3.montoTotal = Math.Round(f.reteiva, decimales).ToString();
                //    factura.impuestosTotales[reten] = impuestoGeneralTOTAL3;
                //    reten++;
                //}
            }

            #endregion

            #region informacionAdicional
            factura.informacionAdicional = null;



            var infoadic = "";
            var infocoment = "";

            if (!string.IsNullOrEmpty(f.numordencompra))
                if (f.numordencompra.Length > 0)
                {
                    infoadic = infoadic + "/ Orden Compra: " + f.numordencompra.ToString();
                }

            if (!string.IsNullOrEmpty(f.numremision))
                if (f.numremision.Length > 0)
                {
                    infoadic = infoadic + "/ Remision: " + f.numremision.ToString();
                }

            if (!string.IsNullOrEmpty(f.comentario))
                if (f.comentario.Length > 0)
                {
                    infocoment = f.comentario;
                }

            var cantinfadic = 0;
            if (infoadic.Length > 0)
            {
                cantinfadic++;
            }

            if (infocoment.Length > 0)
            {
                cantinfadic++;
            }

            if (cantinfadic > 0)
            {
                var lineasinfoadic = new string[1];

                lineasinfoadic[0] = infoadic + "/" + infocoment;
                //lineasinfoadic[1] = infocoment;

                factura.informacionAdicional = lineasinfoadic;

            }

            //LineaInformacionAdicional ad = new LineaInformacionAdicional();

            #endregion

            #region mediosDePago


            var formaspago = new List<formapago>();

            formaspago = fc.getfacturapago(tipodoc, numero);

            factura.mediosDePago = new MediosDePago[formaspago.Count()];
            var countfp = 0;
            foreach (var fp in formaspago)
            {

                MediosDePago medioPago1 = new MediosDePago();
                medioPago1.codigoBanco = null;
                medioPago1.codigoCanalPago = null;
                medioPago1.codigoReferencia = null;
                medioPago1.extras = null;
                medioPago1.fechaDeVencimiento = fp.fecha.ToString("yyyy-MM-dd");
                medioPago1.medioPago = "ZZZ";  //10 efectivo  4 consignacion bancaria 20 cheque
                if (fp.dias == 0)
                {
                    medioPago1.metodoDePago = "1";  //1 CONTADO 2 CREDITO
                }
                else
                {
                    medioPago1.metodoDePago = "2";
                }
                medioPago1.nombreBanco = null;
                medioPago1.numeroDeReferencia = "01";
                medioPago1.numeroDias = fp.dias.ToString();
                medioPago1.numeroTransferencia = null;
                factura.mediosDePago[countfp] = medioPago1;
                countfp++;
            }
            #endregion

            factura.moneda = "COP";

            #region ordenDeCompra
            factura.ordenDeCompra = null;
            #endregion

            f.resnumdesde = Convert.ToInt32(ConfigurationManager.AppSettings.Get("numdesde"));
            f.resmunhasta = Convert.ToInt32(ConfigurationManager.AppSettings.Get("numhasta"));

            factura.propina = null;
            var rango = f.resnumdesde.ToString() + "-" + f.resmunhasta.ToString();

            factura.rangoNumeracion = f.tipodoc.Trim() + "-" + f.resnumdesde.ToString(); //"SETT-" + f.resnumdesde.ToString();
            factura.redondeoAplicado = "0.00";

            #region tasaDeCambio
            factura.tasaDeCambio = null;
            #endregion

            #region tasaDeCambioAlternativa
            factura.tasaDeCambioAlternativa = null;
            #endregion

            #region terminosEntrega
            factura.terminosEntrega = null;
            #endregion

            // TIPO TRANSACCION
            if (f.prefijo == ConfigurationManager.AppSettings.Get("prefijofe"))
            {
                factura.tipoDocumento = "01";  //1 FACTURA VENTA NACIONAL  2 Factura de Exportación  3 Factura de Contingencia 91 Nota Crédito (Exclusivo en referencias a documentos) 92 Nota Débito (Exclusivo en referencias a documentos)
            }

            if (f.prefijo == ConfigurationManager.AppSettings.Get("prefijond"))
            {
                factura.tipoDocumento = "92"; //nota debito
            }

            if (f.prefijo == ConfigurationManager.AppSettings.Get("prefijonc"))
            {
                factura.tipoDocumento = "91"; //nota credito
            }


            factura.tipoOperacion = "10";  //GENERICA  //Servicios AIU
            factura.totalAnticipos = null;
            factura.totalBaseImponible = Math.Round(f.subtotal + totbasimpMuestraG, decimales).ToString(); //revisar para aplicacion AIU
            factura.totalBrutoConImpuesto = Math.Round(f.subtotal + f.impuesto, decimales).ToString();
            factura.totalCargosAplicados = null;
            //factura.totalDescuentos = null;
            //if (itmbon > 0)
            //{
            //    factura.totalDescuentos = Math.Round(f.descuento, decimales).ToString();
            //}
            factura.totalMonto = Math.Round(f.subtotal + f.impuesto, decimales).ToString();
            factura.totalProductos = detalles.Count().ToString();
            factura.totalSinImpuestos = Math.Round(f.subtotal, decimales).ToString();



            return factura;
        }
        #endregion


        #region Enviar (Web Service SOAP Emisión)
        public respuestaElectronica EnviarFactura(string tipodoc, int numero)
        {
            string resultado = "";



            FacturaGeneral factura = BuildFactura(tipodoc, numero); // Se invoca el metodo para construir el objeto factura

            //var f = context.gdtventaenc.Find(id);

            //var empresa = context.gdtempresas.Find(f.gdtempresaid);

            var valido = true;


            //if (!validacorreo(f.gdttercero.correoe))
            //    valido = false;


            //var cnfg = context.gdtempresafacte.Where(c => c.gdtempresaid == empresa.gdtempresaid && c.activo == true).First();



            cnfg.ruta_emision = ConfigurationManager.AppSettings.Get("urlservice");
            cnfg.ruta_adjuntos = ConfigurationManager.AppSettings.Get("urladjuntos");
            cnfg.tokenempresa = ConfigurationManager.AppSettings.Get("token");
            cnfg.tokenpassword = ConfigurationManager.AppSettings.Get("tokenpassword");

            //StreamWriter MyFile = new StreamWriter(@"Request_factura.txt"); //ruta y name del archivo request a almecenar
            //XmlSerializer Serializer1 = new XmlSerializer(typeof(FacturaGeneral));
            //Serializer1.Serialize(MyFile, factura); // Objeto serializado
            //MyFile.Close();

            DocumentResponse docRespuesta; //objeto Response del metodo enviar

            respuestaElectronica resp = new respuestaElectronica();

            inicializar(cnfg.ruta_emision, cnfg.ruta_adjuntos);

            try
            {
                //if (valido)
                //{
                docRespuesta = serviceClient.Enviar(cnfg.tokenempresa, cnfg.tokenpassword, factura, "0");
                //}
                //else
                //{
                //    docRespuesta = new DocumentResponse();
                //    docRespuesta.codigo = 0;
                //    docRespuesta.mensaje = "Verificar correo electronico";
                //}


                //en caso de enviar adjunto
                //docRespuesta = serviceClient.Enviar(tokenEmpresa, tokenAuthorizacion, factura, "1");
                //EnviarArchivosAdjuntos(1, docRespuesta);



                resp.codigo = docRespuesta.codigo;
                resp.consecutivo = docRespuesta.consecutivoDocumento;
                resp.cufe = docRespuesta.cufe;
                resp.mensaje = docRespuesta.mensaje;
                resp.resultado = docRespuesta.resultado;
                resp.mensajevalidacion = docRespuesta.mensajesValidacion;


                //StringBuilder sb = new StringBuilder();

                //sb.Append("Consecutivo Documento: " + docRespuesta.consecutivoDocumento + Environment.NewLine);
                //sb.Append(docRespuesta.codigo + Environment.NewLine);
                //sb.Append(docRespuesta.cufe + Environment.NewLine);
                //sb.Append(docRespuesta.mensaje + Environment.NewLine);
                //sb.Append(docRespuesta.resultado + Environment.NewLine);

                //sb.Append("-------------------------------------------------------" + Environment.NewLine);

                //File.AppendAllText("log.txt", sb.ToString());
                //sb.Clear();


                if (docRespuesta.codigo == 200)
                {
                    resultado = "Codigo: " + docRespuesta.codigo.ToString() + Environment.NewLine +
                                           "Consecutivo Documento: " + docRespuesta.consecutivoDocumento + Environment.NewLine +
                                           "Cufe: " + docRespuesta.cufe + Environment.NewLine +
                                           "Mensaje: " + docRespuesta.mensaje + Environment.NewLine +
                                           "Resultado: " + docRespuesta.resultado;
                    var validaciones = docRespuesta.mensajesValidacion;
                }
                else
                {
                    if (docRespuesta.codigo == 109)
                    {
                        //Value cannot be null.\r\nParameter name: input

                        foreach (var d in docRespuesta.mensajesValidacion)
                        {
                            resp.mensaje = resp.mensaje +", "+  d;
                        }
                    }
                    else
                    {
                        //var docfe = ConfigurationManager.AppSettings.Get("prefijoHK");
                        var respEstado = EstadoDocumento(tipodoc, numero);

                        if (respEstado.codigo == 200)
                        {
                            resp.cufe = respEstado.cufe;

                        }
                    }

                    resultado = "Codigo: " + resp.codigo.ToString() + Environment.NewLine +
                                           "Mensaje: " + resp.mensaje + Environment.NewLine +
                                           "Cufe:" + resp.cufe.Trim() +
                                           "Resultado: " + resp.resultado;


                }
            }
            catch (Exception e)
            {

            }


            return resp;
        }
        #endregion



        #region EstadoDocumento (Web Service SOAP Emisión)
        public respuestaElectronica EstadoDocumento(string tipo, int numero)
        {
            string resultado = "";
            //FacturaGeneral factura = BuildFactura(id); // Se invoca el metodo para construir el objeto factura

            //var cnfg = new config();
            //StreamWriter MyFile = new StreamWriter(@"Request_factura.txt"); //ruta y name del archivo request a almecenar
            //XmlSerializer Serializer1 = new XmlSerializer(typeof(FacturaGeneral));
            //Serializer1.Serialize(MyFile, factura); // Objeto serializado
            //MyFile.Close();

            //DocumentResponse docRespuesta; //objeto Response del metodo enviar

            respuestaElectronica resp = new respuestaElectronica();

            cnfg.ruta_emision = ConfigurationManager.AppSettings.Get("urlservice");
            cnfg.ruta_adjuntos = ConfigurationManager.AppSettings.Get("urladjuntos");
            cnfg.tokenempresa = ConfigurationManager.AppSettings.Get("token");
            cnfg.tokenpassword = ConfigurationManager.AppSettings.Get("tokenpassword");

            inicializar(cnfg.ruta_emision, cnfg.ruta_adjuntos);

            DocumentStatusResponse docRespuesta = serviceClient.EstadoDocumento(cnfg.tokenempresa.Trim(), cnfg.tokenpassword.Trim(), tipo + numero.ToString());

            if (docRespuesta.codigo == 200)
            {
                if (docRespuesta.cufe.Length > 0)
                {
                    //f.cufe = docRespuesta.cufe;
                    //f.electprocesado = true;
                    //f.estado = 2;

                    //context.SaveChanges();

                    //actualiza estado de factura

                    resp.codigo = 200;
                    resp.cufe = docRespuesta.cufe;


                }

                resultado = "Codigo: " + docRespuesta.codigo.ToString() + Environment.NewLine +
                                       "Consecutivo Documento: " + docRespuesta.consecutivo + Environment.NewLine +
                                       "Cufe: " + docRespuesta.cufe + Environment.NewLine +
                                       "Mensaje: " + docRespuesta.mensaje + Environment.NewLine +
                                       "Resultado: " + docRespuesta.resultado;
                var validaciones = docRespuesta.mensaje;
            }
            else
            {
                resultado = "Codigo: " + docRespuesta.codigo.ToString() + Environment.NewLine +
                                       "Mensaje: " + docRespuesta.mensaje + Environment.NewLine +
                                       "Resultado: " + docRespuesta.resultado;
            }


            return resp;
            //MessageBox.Show(resp.codigo + Environment.NewLine + resp.estatusDocumento + Environment.NewLine + resp.mensaje, "Estado de Documento");
        }
        #endregion



        #region DescargaXML_PDF (Web Service SOAP Emisión)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id factura</param>
        /// <param name="tipo">PDF / XML</param>
        public byte[] DescargarXML_PDF(string tipodoc, int numero, string tipo = "PDF")
        {



            //var cnfg = new config();


            cnfg.ruta_emision = ConfigurationManager.AppSettings.Get("urlservice");
            cnfg.ruta_adjuntos = ConfigurationManager.AppSettings.Get("urladjuntos");
            cnfg.tokenempresa = ConfigurationManager.AppSettings.Get("token");
            cnfg.tokenpassword = ConfigurationManager.AppSettings.Get("tokenpassword");

            inicializar(cnfg.ruta_emision, cnfg.ruta_adjuntos);

            DownloadPDFResponse pdfResponse;
            DownloadXMLResponse xmlResponse;
            if (tipo.Equals("PDF"))
            {
                pdfResponse = serviceClient.DescargaPDF(cnfg.tokenempresa, cnfg.tokenpassword, tipodoc + numero.ToString());

                //MessageBox.Show(pdfResponse.codigo + Environment.NewLine + pdfResponse.mensaje, "DEscarga de PDF");
                if (pdfResponse.codigo == 200)
                {

                    return Convert.FromBase64String(pdfResponse.documento);
                    //File.WriteAllBytes(f.gdttipodoc.prefjo+"-"+f.numero + ".pdf", Convert.FromBase64String(pdfResponse.documento));
                }
            }
            else
            {
                xmlResponse = serviceClient.DescargaXML(cnfg.tokenempresa, cnfg.tokenpassword, tipodoc + numero.ToString());
                //MessageBox.Show(xmlResponse.codigo + Environment.NewLine + xmlResponse.mensaje, "Descarga de XML");
                if (xmlResponse.codigo == 200)
                {
                    return Convert.FromBase64String(xmlResponse.documento);
                    //File.WriteAllBytes(f.gdttipodoc.prefjo + "-" + f.numero+ ".xml", Convert.FromBase64String(xmlResponse.documento));
                }
            }

            return null;
        }

        #endregion

        #region validaciones

        private Boolean validacorreo(String email)
        {
            String expresion;
            expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            if (Regex.IsMatch(email, expresion))
            {
                if (Regex.Replace(email, expresion, String.Empty).Length == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region Ftech

        public class RespuestaWebservice
        {
            public string code = "";
            public string succes = "";
            public string transaccionID = "";
            public string MsgError = "";
        }
        public class RespuestaDocs
        {
            public string code = "";
            public string succes = "";
            public string status = "";
            public string MsgError = "";
        }
        public class RespuestaXML
        {
            public string code = "";
            public string succes = "";
            public string resourceData = "";
            public string MsgError = "";
        }

        public class RespuestaGral
        {
            public string MsgGRAL = "";
        }


        public static RespuestaGral UploadInvoiceFtech(string UserName, string Password, string XML)
        {
            WebRequest Request;
            WebResponse Response;
            Stream DataStream;
            StreamReader Reader;
            byte[] SoapByte;
            RespuestaGral Respuestaws = new RespuestaGral();
            bool pSuccess = true;

            // System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            SoapByte = System.Text.Encoding.UTF8.GetBytes(GenerarRequestUploadFtech(UserName, Password, XML).ToString());
            Request = WebRequest.Create("https://ws.facturatech.co/21/index.php");
            Request.Headers.Add("SOAPAction", "urn:https://ws.facturatech.co/21#FtechAction.uploadInvoiceFile");
            Request.ContentType = "text/xml; charset=utf-8";
            Request.ContentLength = SoapByte.Length;
            Request.Method = ("POST");
            DataStream = Request.GetRequestStream();
            DataStream.Write(SoapByte, 0, SoapByte.Length);
            DataStream.Close();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

            try
            {
                Response = Request.GetResponse();

                DataStream = Response.GetResponseStream();
                Reader = new StreamReader(DataStream, encode);
                string SD2Request = Reader.ReadToEnd();
                DataStream.Close();
                Reader.Close();
                Response.Close();
                DataStream.Dispose();
                Reader.Dispose();

                SD2Request = SD2Request.Replace("&lt;", "<");
                SD2Request = SD2Request.Replace("&gt;", ">");
                SD2Request = SD2Request.Replace("&amp;", "&");
                SD2Request = SD2Request.Replace("&quot;", "\"");
                SD2Request = SD2Request.Replace("&nbsp;", " ");
                SD2Request = SD2Request.Replace("&apos;", "'");


                // Dim Respuestaws As New RespuestaWebservice

                Respuestaws = InterpretarRequestUpload(SD2Request);
            }
            catch (Exception ex)
            {
            }
            return Respuestaws;

            FreeMemory.FlushMemory();
        }



        private static object GenerarRequestUploadFtech(string UserName, string Password, string XML)
        {


            string XMLB64 = Convert.ToBase64String(new ASCIIEncoding().GetBytes(XML));


            string Request = "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns=\"urn:https://ws.facturatech.co/21\">" + "\r\n" +
        "<soapenv:Header/>" + "\r\n" +
        "<soapenv:Body>" + "\r\n" +
            "<ns:FtechAction.uploadInvoiceFile soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" + "\r\n" +
                "<username xsi:type=\"xsd:string\"></username>" + "\r\n" +
                "<password xsi:type=\"xsd:string\"></password>" + "\r\n" +
                "<xmlBase64 xsi:type=\"xsd:string\"></xmlBase64>" + "\r\n" +
            "</ns:FtechAction.uploadInvoiceFile>" + "\r\n" +
        "</soapenv:Body>" + "\r\n" +
    "</soapenv:Envelope>";

            XmlDocument RequesXML = new XmlDocument();
            RequesXML.LoadXml(Request);

            //XmlNode Nodo;
            foreach (XmlNode /*TODO: cambiar el nombre de la variable*/ Nodo in RequesXML)
            {
                try
                {

                    //  Dim AUX = sFechaSAT.Replace("-", "")
                    Nodo["soapenv:Body"]["ns:FtechAction.uploadInvoiceFile"]["username"].InnerText = UserName;
                    Nodo["soapenv:Body"]["ns:FtechAction.uploadInvoiceFile"]["password"].InnerText = Password;
                    Nodo["soapenv:Body"]["ns:FtechAction.uploadInvoiceFile"]["xmlBase64"].InnerText = XMLB64;

                    //A(1) = companyId & AUX.Replace(":", "")
                }
                catch (Exception ex)
                {


                }
            }
            return RequesXML.OuterXml;
        }
        private static object GenerarRequesDocumentStatusFtech(string UserName, string Password, string transaccionID)
        {
            string Request = "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns=\"urn:https://ws.facturatech.co/21\">" + "\r\n" +
        "<soapenv:Header/>" + "\r\n" +
        "<soapenv:Body>" + "\r\n" +
            "<ns:FtechAction.documentStatusFile soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" + "\r\n" +
                "<username xsi:type=\"xsd:string\">?</username>" + "\r\n" +
                "<password xsi:type=\"xsd:string\">?</password>" + "\r\n" +
                "<transaccionID xsi:type=\"xsd:string\">?</transaccionID>" + "\r\n" +
            "</ns:FtechAction.documentStatusFile>" + "\r\n" +
        "</soapenv:Body>" + "\r\n" +
    "</soapenv:Envelope>";

            XmlDocument RequesXML = new XmlDocument();
            RequesXML.LoadXml(Request);

            //  XmlNode Nodo;
            foreach (XmlNode /*TODO: cambiar el nombre de la variable*/ Nodo in RequesXML)
            {
                try
                {
                    Nodo["soapenv:Body"]["ns:FtechAction.documentStatusFile"]["username"].InnerText = UserName;
                    Nodo["soapenv:Body"]["ns:FtechAction.documentStatusFile"]["password"].InnerText = Password;
                    Nodo["soapenv:Body"]["ns:FtechAction.documentStatusFile"]["transaccionID"].InnerText = transaccionID;
                }
                catch (Exception ex)
                {

                }
            }
            return RequesXML.OuterXml;
        }
        private static object GenerarRequesDownloadXML(string UserName, string Password, string Prefijo, string Folio)
        {
            string Request = "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns=\"urn:https://ws.facturatech.co/21\">" + "\r\n" +
        "<soapenv:Header/>" + "\r\n" +
        "<soapenv:Body>" + "\r\n" +
            "<ns:FtechAction.downloadXMLFile soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" + "\r\n" +
                "<username xsi:type=\"xsd:string\"></username>" + "\r\n" +
                "<password xsi:type=\"xsd:string\"></password>" + "\r\n" +
                "<prefijo xsi:type=\"xsd:string\"></prefijo>" + "\r\n" +
                "<folio xsi:type=\"xsd:string\"></folio>" + "\r\n" +
            "</ns:FtechAction.downloadXMLFile>" + "\r\n" +
        "</soapenv:Body>" + "\r\n" +
    "</soapenv:Envelope>";


            XmlDocument RequesXML = new XmlDocument();
            RequesXML.LoadXml(Request);

            // XmlNode Nodo;
            foreach (XmlNode /*TODO: cambiar el nombre de la variable*/ Nodo in RequesXML)
            {
                try
                {
                    Nodo["soapenv:Body"]["ns:FtechAction.downloadXMLFile"]["username"].InnerText = UserName;
                    Nodo["soapenv:Body"]["ns:FtechAction.downloadXMLFile"]["password"].InnerText = Password;
                    Nodo["soapenv:Body"]["ns:FtechAction.downloadXMLFile"]["prefijo"].InnerText = Prefijo;
                    Nodo["soapenv:Body"]["ns:FtechAction.downloadXMLFile"]["folio"].InnerText = Folio;
                }
                catch (Exception ex)
                {

                }
            }

            return RequesXML.OuterXml;
        }

        private static RespuestaGral InterpretarRequestUpload(string Response)
        {
            // Dim cadenaXML = Mid(Response, (InStrRev(Response, "<soap:Envelope")), (InStr(Response, "</soap:Envelope>") - InStrRev(Response, "<soap:Envelope")) + "</soap:Envelope>".Length)
            RespuestaGral respuestaReq = new RespuestaGral();

            XmlDocument RequesXML = new XmlDocument();
            RequesXML.LoadXml(Response);

            // XmlNode Nodo;
            foreach (XmlNode Nodo in RequesXML)
            {
                try
                {
                    string MSGerror = "";
                    string transaccionID = "";
                    string success = "";
                    string code = "";

                    try
                    {
                        MSGerror = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.uploadInvoiceFileResponse"]["return"]["error"].InnerText;

                    }
                    catch (Exception ex)
                    {
                    }
                    try
                    {
                        transaccionID = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.uploadInvoiceFileResponse"]["return"]["transaccionID"].InnerText;
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        success = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.uploadInvoiceFileResponse"]["return"]["success"].InnerText;
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        code = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.uploadInvoiceFileResponse"]["return"]["code"].InnerText;
                    }
                    catch (Exception ex)
                    {

                    }
                    // respuestaReq.MsgError = Nodo.Item("SOAP-ENV:Body").Item("ns1:FtechAction.uploadInvoiceFileResponse").Item("return").Item("error").InnerText
                    // respuestaReq.transaccionID = Nodo.Item("SOAP-ENV:Body").Item("ns1:FtechAction.uploadInvoiceFileResponse").Item("return").Item("transaccionID").InnerText
                    // respuestaReq.succes = Nodo.Item("SOAP-ENV:Body").Item("ns1:FtechAction.uploadInvoiceFileResponse").Item("return").Item("success").InnerText
                    // respuestaReq.code = Nodo.Item("SOAP-ENV:Body").Item("ns1:FtechAction.uploadInvoiceFileResponse").Item("return").Item("code").InnerText
                    respuestaReq.MsgGRAL = MSGerror + "|" + transaccionID + "|" + success + "|" + code;
                }
                catch (Exception ex)
                {
                    try
                    {
                    }
                    catch (Exception ex2)
                    {
                        respuestaReq.MsgGRAL = Response;
                    }
                }
            }

            return respuestaReq;
        }
        private static RespuestaGral InterpretarRequestDocumentStatus(string Response)
        {
            RespuestaGral respuestaReq = new RespuestaGral();

            XmlDocument RequesXML = new XmlDocument();
            RequesXML.LoadXml(Response);

            //XmlNode Nodo;
            foreach (XmlNode Nodo in RequesXML)
            {
                if (Nodo.Name == "SOAP-ENV:Envelope")
                {
                    try
                    {
                        string MSGerror = "";
                        string status = "";
                        string success = "";
                        string code = "";

                        try
                        {
                            MSGerror = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.documentStatusFileResponse"]["return"]["error"].InnerText;
                        }
                        catch (Exception ex)
                        {
                        }
                        try
                        {
                            status = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.documentStatusFileResponse"]["return"]["status"].InnerText;
                        }
                        catch (Exception ex)
                        {
                        }
                        try
                        {
                            success = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.documentStatusFileResponse"]["return"]["success"].InnerText;
                        }
                        catch (Exception ex)
                        {
                        }
                        try
                        {
                            code = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.documentStatusFileResponse"]["return"]["code"].InnerText;
                        }
                        catch (Exception ex)
                        {
                        }


                        respuestaReq.MsgGRAL = MSGerror + "|" + status + "|" + success + "|" + code;
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            return respuestaReq;
        }
        private static RespuestaGral InterpretarRequestDownloadXML(string Response)
        {
            RespuestaGral respuestaReq = new RespuestaGral();

            XmlDocument RequesXML = new XmlDocument();
            RequesXML.LoadXml(Response);

            // XmlNode Nodo;
            foreach (XmlNode Nodo in RequesXML)
            {
                if (Nodo.Name == "SOAP-ENV:Envelope")
                {
                    try
                    {
                        string MSGerror = "";
                        string success = "";
                        string resourceData = "";
                        string code = "";

                        try
                        {
                            MSGerror = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.downloadXMLFileResponse"]["return"]["return"].InnerText;
                        }
                        catch (Exception ex)
                        {
                        }

                        try
                        {
                            byte[] b = Convert.FromBase64String(Nodo["SOAP-ENV:Body"]["ns1:FtechAction.downloadXMLFileResponse"]["return"]["resourceData"].InnerText);
                            resourceData = System.Text.Encoding.UTF8.GetString(b);
                        }

                        catch (Exception ex)
                        {
                        }

                        try
                        {
                            success = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.downloadXMLFileResponse"]["return"]["success"].InnerText;
                        }
                        catch (Exception ex)
                        {
                        }
                        try
                        {
                            code = Nodo["SOAP-ENV:Body"]["ns1:FtechAction.downloadXMLFileResponse"]["return"]["code"].InnerText;
                        }
                        catch (Exception ex)
                        {
                        }

                        respuestaReq.MsgGRAL = MSGerror + "|" + resourceData + "|" + success + "|" + code;
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            return respuestaReq;
        }


        internal static class FreeMemory
        {
            [System.Runtime.InteropServices.DllImport("kernel32.dll")]
            private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);

            public static void FlushMemory()
            {
                try
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    if ((Environment.OSVersion.Platform == PlatformID.Win32NT))
                        SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                }
                catch (Exception ex)
                {
                }
            }
        }





        #endregion


    }
}