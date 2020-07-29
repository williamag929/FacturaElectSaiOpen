using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FacturaElectSaiOpen
{
    public partial class FrmVistaGrl : Form
    {
        public FrmVistaGrl()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbDocumento.SelectedIndex = 0;

            dtpDesde.Value = DateTime.Today;
            dtpHasta.Value = DateTime.Today;

            GetFacturas();


        }

        private void GetFacturas()
        {
            var tipofe = ConfigurationManager.AppSettings.Get("prefijofe");
            var tiponc = ConfigurationManager.AppSettings.Get("prefijonc");
            var tipond = ConfigurationManager.AppSettings.Get("prefijond");


            FbData fb = new FbData();

            var sql = "select a.fecha,TIPDOC.SIGLA AS tipo,a.number numero,b.id_n nit,b.company Cliente,a.subtotal,a.salestax impuesto, " +
                " a.total, coalesce(a.procesadodian, 'N') procesado, coalesce(a.cufe, '') cufe " +
                 " from oe a,tipdoc, cust b where a.tipo = tipdoc.clase and a.id_n = b.id_n and a.fecha between '" + dtpDesde.Value.ToString("yyyy-MM-dd") + "' and '" + dtpHasta.Value.ToString("yyyy-MM-dd") + "' ";


            if (cmbDocumento.SelectedIndex == 0)
                sql = sql + "and TIPDOC.SIGLA  = '" + tipofe + "' ";


            if (cmbDocumento.SelectedIndex == 1)
                sql = sql + "and TIPDOC.SIGLA = '" + tipond + "' ";


            if (cmbDocumento.SelectedIndex == 2)
                sql = sql + "and TIPDOC.SIGLA = '" + tiponc + "' ";


            var da = fb.DataReader(sql);


            DataTable dt = new DataTable();

            da.Fill(dt);

            dataGridView1.DataSource = dt;
        }

        private void enviadoc(string tipodoc, int numero)
        {

            FacturaElectronica fe = new FacturaElectronica();

            var respuesta = fe.EnviarFactura(tipodoc, numero);

            //cargar encabezado

            //cargar detalles

            //cargar formapago

        }

        private void btnEmitir_Click(object sender, EventArgs e)
        {
            //procesa factura
            if (dataGridView1.SelectedRows.Count > 0)
            {
                FbData fb = new FbData();

                string sql = "";
                string clase = "";




                var tipo = dataGridView1.SelectedRows[0].Cells[1].Value;
                var numero = dataGridView1.SelectedRows[0].Cells[2].Value;


                #region aplicaredondeoimpuesto

                clase = Getclasefactura(tipo);

                if (chkImpuesto.Checked)
                {

                    sql = "update oedet set vlr_iva = extend * porc_iva / 100 where tipo = '" + clase + "' and number = " + numero.ToString();

                    fb.ExecuteNonQuery(sql);

                    sql = "update oe set salestax = (select sum(vlr_iva) from oedet where oedet.tipo = oe.tipo and oedet.number = oe.number) " +
                           " where oe.tipo = '" + clase + "' and number = " + numero.ToString();

                    fb.ExecuteNonQuery(sql);

                    sql = "update oe set total = subtotal + salestax - disc1 - disc2 - disc3 " +
                    " where oe.tipo = '" + clase + "' and number = " + numero.ToString();

                    fb.ExecuteNonQuery(sql);
                }


                #endregion

                FacturaElectronica fe = new FacturaElectronica();


                #region validar datos

                var f = new FacturaEnc();

                FbData db = new FbData();

                var sqlciiu = "select FIRST 1 actividad_eco_enc.cod_internacional " +
                " from actividad_eco_enc, actividad_eco_det " +
                " where actividad_eco_enc.codact = actividad_eco_det.codact " +
                " and actividad_eco_det.principal = 'S' " +
                " and actividad_eco_det.id_n = OE.ID_N";


                sql = "SELECT OE.ID_EMPRESA, " +
                "OE.ID_SUCURSAL, TIPDOC.SIGLA AS TIPO,OE.NUMBER, OE.FECHA, OE.DUEDATE FECHAVENC, CUST.ID_N, CUST.CV,CUST.COMPANY, CUST.ADDR1 direccion,CUST.PHONE1 telefono1, " +
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

                try
                {


                    var r = dt.Rows[0];

                    var correoe = r["email"].ToString().Trim();

                    if (correoe =="")
                    {
                        MessageBox.Show("Verificar correo electronico");
                    }

                    var tipodoident = r["CODTIPOIDENTIFICACION"].ToString();
                    var ciiu = r["CODIGOCIIU"].ToString();

                    if (tipodoident == "31" && ciiu =="" )
                    {
                        MessageBox.Show("Verificar codigo ciiu tercero con nit");
                    }

                }
                catch(Exception error)
                {
                    MessageBox.Show("Verificar datos tributarios/direccion de envio");
                }

                #endregion



                var data = fe.EnviarFactura(tipo.ToString().Trim(), Convert.ToInt32(numero));

                MessageBox.Show(tipo + "-" + numero.ToString() + data.mensaje);

                if (data.codigo == 200 || data.codigo == 201)
                {


                    //string clase =   Getclasefactura(tipo);

                    sql = "update oe set cufe = '" + data.cufe + "', procesadoDian ='S' where tipo='" + clase + "' and number=" + numero.ToString();

                    fb.ExecuteNonQuery(sql);


                }

                LogResult(tipo.ToString(), numero.ToString(), data);
            }
            else
            {
                MessageBox.Show("Seleccione una fila");
            }

            GetFacturas();
        }


        /// <summary>
        /// recibe la sigla del documento, retorna la clase
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public string Getclasefactura(object tipo)
        {
            FbData db = new FbData();

            var sqltipodoc = "select clase from tipdoc where sigla = '" + tipo + "'";

            var da = db.DataReader(sqltipodoc);

            DataTable dt = new DataTable();

            da.Fill(dt);

            var clase = dt.Rows[0][0].ToString();

            return clase;

        }



        private static void LogResult(string tipo, string numero, respuestaElectronica data)
        {
            var m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {

                    try
                    {
                        w.Write("\r\nLog Entry : {0}-{1} ", tipo, numero);
                        w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                            DateTime.Now.ToLongDateString());
                        w.WriteLine(" mensaje  :{0}", data.mensaje);
                        w.WriteLine(" resultado :{0}", data.resultado);

                        if (data.mensajevalidacion.Count() > 0)
                        {
                            foreach (var record in data.mensajevalidacion)
                            {
                                w.WriteLine("  -{0}", record);
                            }
                        }

                        w.WriteLine("-------------------------------");
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }


        private static void LogText(string tipo, string numero, string data)
        {
            var m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {

                    try
                    {
                        w.Write("\r\nLog Entry : {0}-{1} ", tipo, numero);
                        w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                            DateTime.Now.ToLongDateString());
                        w.WriteLine(" mensaje  :{0}", data);

                        w.WriteLine("-------------------------------");
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }


        private void Fill(object sender, EventArgs e)
        {
            GetFacturas();
        }

        private void btnXml_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var tipo = dataGridView1.SelectedRows[0].Cells[1].Value.ToString().Trim();
                var numero = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[2].Value);



                FacturaElectronica fe = new FacturaElectronica();
                var data = fe.DescargarXML_PDF(tipo, numero, "XML");


               try
                {
                    SaveFileDialog SaveFileDialog1 = new SaveFileDialog();

                    SaveFileDialog1.Title = "Browse XML Files";

                    SaveFileDialog1.DefaultExt = ".xml";
                    SaveFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    SaveFileDialog1.FilterIndex = 1;

                    if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        var filename = SaveFileDialog1.FileName;


                        File.WriteAllBytes(filename, data);
                        //FileStream fs = File.Create(filename);
                        //BinaryWriter bw = new BinaryWriter(fs);

                        System.Diagnostics.Process.Start(filename);

                        MessageBox.Show("Archivo null.");
                    }
                }
                catch
                {
                    MessageBox.Show("Archivo null.");

                    LogText(tipo.ToString(), numero.ToString(), "Archivo vacio");

                    // LogResult(tipo.ToString(), numero.ToString(), "Archivo vacio");
                }
            }
            else
            {
                MessageBox.Show("Seleccione una fila");
            }


        }

        private void btnPDF_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var tipo = dataGridView1.SelectedRows[0].Cells[1].Value.ToString().Trim();
                var numero = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[2].Value);

                //tipo = "SETT";
                //numero = 3;

                FacturaElectronica fe = new FacturaElectronica();
                var data = fe.DescargarXML_PDF(tipo.ToString(), numero);

                try
                { 

                    SaveFileDialog SaveFileDialog1 = new SaveFileDialog();

                    SaveFileDialog1.Title = "Browse PDF Files";

                    SaveFileDialog1.DefaultExt = ".pdf";

                    SaveFileDialog1.Filter = "Pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
                    SaveFileDialog1.FilterIndex = 1;

                    if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        var filename = SaveFileDialog1.FileName;


                        File.WriteAllBytes(filename, data);
                        //FileStream fs = File.Create(filename);
                        //BinaryWriter bw = new BinaryWriter(fs);
                        System.Diagnostics.Process.Start(filename);

                    }
                }
                catch
                {
                    MessageBox.Show("Archivo null.");
                }



            }
            else
            {
                MessageBox.Show("Seleccione una fila");
            }

        }

        private void btnEstado_Click(object sender, EventArgs e)
        {

            if (dataGridView1.SelectedRows.Count > 0)
            {
                var tipo =  dataGridView1.SelectedRows[0].Cells[1].Value.ToString().Trim();
                var numero = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[2].Value);

                Regex reg = new Regex("[*'\",_&#^@]");
                tipo = reg.Replace(tipo, string.Empty);

                FacturaElectronica fe = new FacturaElectronica();

                var data = fe.EstadoDocumento(tipo, numero);

                MessageBox.Show(tipo + "-" + numero.ToString() +"cufe:"+ data.cufe);

           

                LogResult(tipo.ToString(), numero.ToString(), data);

                if (data.codigo == 200 || data.codigo == 201)
                {

                    FbData fb = new FbData();

                    string clase = Getclasefactura(tipo);

                    var sql = "update oe set cufe = '" + data.cufe + "', procesadoDian ='S' where tipo='" + clase.Trim() + "' and number="+ numero.ToString();


                    var rowsaffect = fb.ExecuteNonQuery(sql);
                   
                    


                }

                GetFacturas();

            }
            else
            {
                MessageBox.Show("Seleccione una fila");
            }



        }

        private void button2_Click(object sender, EventArgs e)
        {

            var m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!File.Exists(m_exePath + "\\" + "log.txt"))
            {
                System.Diagnostics.Process.Start(m_exePath + "\\" + "log.txt");
            }
        }
    }
}
