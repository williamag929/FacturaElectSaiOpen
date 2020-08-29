using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturaElectSaiOpen
{
    class facturaController
    {
        public FacturaEnc getfacturaenc(string tipo, int numero)
        {
            var f = new FacturaEnc();

            FbData db = new FbData();

            var sqlciiu = "select FIRST 1 actividad_eco_enc.cod_internacional " +
            " from actividad_eco_enc, actividad_eco_det " +
            " where actividad_eco_enc.codact = actividad_eco_det.codact " +
            " and actividad_eco_det.principal = 'S' " +
            " and actividad_eco_det.id_n = OE.ID_N";


            var sql = "SELECT OE.ID_EMPRESA, " +
            "OE.ID_SUCURSAL, TIPDOC.SIGLA AS TIPO,OE.NUMBER, OE.FECHA, OE.DUEDATE FECHAVENC, CUST.ID_N, CUST.CV,CUST.COMPANY, SHIPTO.ADDR1 direccion,SHIPTO.PHONE1 telefono1, " +
            " CIUDADES.CODIGO COD_CIUDAD,SHIPTO.CITY,SHIPTO.COD_DPTO,SHIPTO.DEPARTAMENTO,SHIPTO.EMAIL,OE.OCNUMERO,OE.NROREMISION,OE.COMMENTS, " +
            " TRIBUTARIA_TIPODOCUMENTO.TDOC CODTIPOIDENTIFICACION,TRIBUTARIA_TIPODOCUMENTO.DESCRIPCION DESCTIPOIDENTIFICACION, " +
            " TRIBUTARIA_TIPOCONTRIBUYENTE.CODIGO CODTIPOPERSONA,TRIBUTARIA_TIPOCONTRIBUYENTE.DESCRIPCION DESCRIPCIONTIPOPERSONA, " +
            "(OE.SUBTOTAL - OE.DESTOTAL) VLRBRUTO,OE.SUBTOTAL,OE.SALESTAX IMPUESTO,OE.TOTAL,coalesce(OE.PORCRTFTE,0) PORCRTFTE,coalesce(OE.DISC1,0) retefuente, " +
            " coalesce(OE.DISC3,0) RETEIVA,coalesce(OE.DISC2,0) RETEICA,OE.DESTOTAL,coalesce((" + sqlciiu + "),'') CODIGOCIIU,OE.DEV_FACTURA CRUCENUMERO, OE.DEV_TIPOFAC CRUCETIPO,OE.CUFE " +
            "FROM CUST,OE,SHIPTO,TRIBUTARIA,TRIBUTARIA_TIPOCONTRIBUYENTE,TRIBUTARIA_TIPODOCUMENTO,TIPDOC,CIUDADES " +
            "WHERE((CUST.ID_N = OE.ID_N) " +
            " AND(CUST.ID_N = SHIPTO.ID_N) " +
            " AND(OE.ID_N = TRIBUTARIA.ID_N) " +
            " AND(OE.SHIPTO = SHIPTO.SUCCLIENTE) " +
            " AND(SHIPTO.CITY = CIUDADES.CIUDAD AND CIUDADES.ID_DEPTO = SHIPTO.COD_DPTO) " +
            " AND (OE.TIPO = TIPDOC.CLASE) " +
            " AND(TRIBUTARIA.TDOC = TRIBUTARIA_TIPODOCUMENTO.TDOC) " +
            " AND(TRIBUTARIA.TIPO_CONTRIBUYENTE = TRIBUTARIA_TIPOCONTRIBUYENTE.CODIGO OR TRIBUTARIA.TIPO_CONTRIBUYENTE = 0 )) " +
            " AND(((OE.ID_EMPRESA = 1) " +
            " AND(TIPDOC.SIGLA = '" + tipo + "') " +
            " AND(OE.NUMBER = " + numero + ")))";


            var da = db.DataReader(sql);

            DataTable dt = new DataTable();

            da.Fill(dt);

            var r = dt.Rows[0];

            f.numero = Convert.ToInt32(r["number"].ToString());
            f.tipodoc = r["tipo"].ToString().Trim();
            f.prefijo = r["tipo"].ToString().Trim();
            f.fecha = Convert.ToDateTime(r["fecha"].ToString());
            f.fechavence = Convert.ToDateTime(r["fechavenc"].ToString());
            f.nit = r["id_n"].ToString().Trim();
            f.dv = r["cv"].ToString().Trim();
            f.company = r["company"].ToString().Trim();
            f.direccion = r["direccion"].ToString().Trim();
            f.telefono1 = r["telefono1"].ToString().Trim().Replace(" ", "");
            f.codciudad = r["cod_ciudad"].ToString().Trim();
            f.ciudad = r["city"].ToString().Trim();
            f.coddepto = r["cod_dpto"].ToString().Trim();
            f.departamento = r["departamento"].ToString().Trim();
            f.correoe = r["email"].ToString().Trim();
            f.numordencompra = r["ocnumero"].ToString().Trim();
            f.numremision = r["nroremision"].ToString().Trim();
            f.comentario = r["comments"].ToString().Trim();
            f.tipoIdentificacion = r["CODTIPOIDENTIFICACION"].ToString();
            f.tipoPersona = r["CODTIPOPERSONA"].ToString();
            f.vlrbruto = Math.Abs(Convert.ToDecimal(r["vlrbruto"].ToString()));
            f.subtotal = Math.Abs(Convert.ToDecimal(r["subtotal"].ToString()));
            f.impuesto = Math.Abs(Convert.ToDecimal(r["impuesto"].ToString()));
            f.total = Math.Abs(Convert.ToDecimal(r["total"].ToString()));
            f.baseretencion = Math.Abs(Convert.ToDecimal(r["subtotal"].ToString()));
            f.porcretefuente = Math.Abs(Convert.ToDecimal(r["PORCRTFTE"].ToString()));
            f.retefuente = Math.Abs(Convert.ToDecimal(r["retefuente"].ToString()));
            f.porcreteica = 0;//Convert.ToDecimal(r["dv"].ToString());
            f.reteica = Math.Abs(Convert.ToDecimal(r["RETEICA"].ToString()));
            f.porcreteiva = 0;//Convert.ToDecimal(r["dv"].ToString());
            f.reteiva = Math.Abs(Convert.ToDecimal(r["RETEIVA"].ToString()));
            f.descuentos = Convert.ToDecimal(r["DESTOTAL"].ToString());
            f.codigociiu = r["CODIGOCIIU"].ToString();
            f.cufe = r["cufe"].ToString();
            f.crucenumero = r["crucenumero"].ToString();
            f.crucetipo = r["crucetipo"].ToString();


            //List<FacturaEnc> lf = new List<FacturaEnc>();
            //foreach (DataRow row in dt.Rows)
            //{
            //    FacturaEnc fe = new FacturaEnc();
            //    fe.numero = Convert.ToInt32(row["Numero"].ToString());

            //    lf.Add(fe);
            //}


            return f;
        }


        public List<facturadet> getfacturadet(string tipo, int numero)
        {
            var fd = new facturadet();

            FbData db = new FbData();


            var sql = " SELECT OEDET.NUMBER, " +
            " OEDET.TIPO, " +
            " OEDET.ITEM CODIGOITEM, " +
            " ITEM.REFFABRICA, " +
            " ITEM.DESCRIPCION, " +
            " OEDET.QTYSHIP CANTIDAD, " +
            " OEDET.PRICE PRECIO, " +
            " OEDET.PORC_IVA, " +
            " OEDET.VLR_IVA, " +
            " coalesce(OEDET.DCTFIJO,0) DCTPORC, " +
            " coalesce(OEDET.TOTALDCT,0) DESCUENTOVAL, " +
            " (OEDET.EXTEND) SUBTOTAL, " +
            " (OEDET.EXTEND + OEDET.VLR_IVA) TOTAL, " +
            " UNIDAD.EQUIVALENTE AS UNDMEDIDA "+
            "FROM OE,TIPDOC,OEDET, ITEM, UNIDAD " +
            "WHERE OE.TIPO = OEDET.TIPO AND OE.NUMBER = OEDET.NUMBER " +
            "AND(OE.TIPO = TIPDOC.CLASE) " +
            "AND OEDET.ITEM = ITEM.ITEM " +
            "AND OEDET.COD_UNIDAD_VENTA = UNIDAD.COD_UNIDAD " +
            "AND(TIPDOC.SIGLA = '" + tipo + "') " +
            " AND (OE.NUMBER = " + numero + ")";


            var da = db.DataReader(sql);

            DataTable dt = new DataTable();

            da.Fill(dt);


            List<facturadet> lf = new List<facturadet>();
            foreach (DataRow row in dt.Rows)
            {
                facturadet fe = new facturadet();
                fe.codproducto = row["codigoitem"].ToString().Trim();
                fe.referencia = row["reffabrica"].ToString().Trim();
                fe.descprocucto = row["descripcion"].ToString().Trim();
                fe.descalterna = "";
                fe.cantidad = Math.Abs(Convert.ToInt32(row["cantidad"].ToString()));
                fe.precio = Math.Abs(Convert.ToDecimal(row["precio"].ToString()));
                fe.porcimp = Convert.ToDecimal(row["porc_iva"].ToString());
                fe.impuesto = Math.Abs(Convert.ToDecimal(row["vlr_iva"].ToString()));
                fe.unidadmedida = Convert.ToString(row["UNDMEDIDA"].ToString());

                fe.descuentovalor = Math.Abs(Convert.ToDecimal(row["descuentoval"].ToString()));
                fe.subtotal = Math.Abs(Convert.ToDecimal(row["subtotal"].ToString()));
                fe.descuentol = Math.Abs(fe.descuentovalor / (fe.cantidad * fe.precio) * 100);  // Math.Abs(Convert.ToDecimal(row["dctporc"].ToString()));

                fe.total = Math.Abs(Convert.ToDecimal(row["total"].ToString()));

                lf.Add(fe);
            }


            return lf;
        }


        public List<formapago> getfacturapago(string tipo, int numero)
        {
            var fp = new formapago();

            FbData db = new FbData();


            var sql = "  SELECT PAGOS.NUMERO, " +
            " PAGOS.TIPO as tipo, PAGOS.CONCEPTO as concepto, " +
            " PAGOS.DESCRIPCION,   PAGOS.VLR_PAGO as valor, PAGOS.DIAS, PAGOS.FECHA " +
            " FROM OE,TIPDOC,PAGOS " +
            "WHERE OE.TIPO = PAGOS.TIPO AND OE.NUMBER = PAGOS.NUMERO " +
            "AND (OE.TIPO = TIPDOC.CLASE) " +
            "AND (TIPDOC.SIGLA = '" + tipo + "') " +
            " AND (OE.NUMBER = " + numero + ")";


            var da = db.DataReader(sql);

            DataTable dt = new DataTable();

            da.Fill(dt);



            List<formapago> lfp = new List<formapago>();
            foreach (DataRow row in dt.Rows)
            {
                formapago p = new formapago();
                p.concepto = row["concepto"].ToString();
                p.tipo = row["tipo"].ToString();
                p.valor = Math.Abs(Convert.ToDecimal(row["valor"].ToString()));
                p.dias = Convert.ToInt32(row["dias"].ToString());
                p.fecha = Convert.ToDateTime(row["fecha"].ToString());
                lfp.Add(p);
            }


            return lfp;
        }

        /// <summary>
        /// recibe el tipo de documento y retorna la sigla
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public string GetSiglafactura(object tipo)
        {
            FbData db = new FbData();

            var sqltipodoc = "select sigla from tipdoc where clase = '" + tipo + "'";

            var da = db.DataReader(sqltipodoc);

            DataTable dt = new DataTable();

            da.Fill(dt);

            var clase = dt.Rows[0][0].ToString().Trim();

            return clase;

        }

    }
}
