using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace RegistroGastos
{
    public class Gasto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public string Categoria { get; set; }

        public Gasto(int id, string descripcion, decimal monto, string categoria)
        {
            Id = id;
            Descripcion = descripcion;
            Monto = monto;
            Categoria = categoria;
        }

        public override string ToString()
        {
            return $"{Id} {Descripcion} Q {Monto:F2} {Categoria}";
        }
    }

    public class Program
    {
        private static readonly string ArchivoCsv = "gastos.csv";

        public static void Main(string[] args)
        {
            List<Gasto> listaGastos = new List<Gasto>();
            int siguienteId = 1;

            // Cargar datos al iniciar
            CargarDesdeCsv(listaGastos, ref siguienteId);

            bool continuar = true;
            do
            {
                int opcion = LeerOpcionMenu();

                switch (opcion)
                {
                    case 1:
                        AgregarGasto(listaGastos, ref siguienteId);
                        break;
                    case 2:
                        ListarGastos(listaGastos);
                        break;
                    case 3:
                        BuscarPorCategoria(listaGastos);
                        break;
                    case 4:
                        GuardarEnCsv(listaGastos);
                        Console.WriteLine("Saliendo...");
                        continuar = false;
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
                Console.WriteLine();
            } while (continuar);
        }

        private static int LeerOpcionMenu()
        {
            Console.WriteLine("=== REGISTRO DE GASTOS ===");
            Console.WriteLine("1. Agregar gasto");
            Console.WriteLine("2. Listar gastos");
            Console.WriteLine("3. Buscar por categoría");
            Console.WriteLine("4. Salir");
            Console.Write("Elige una opción: ");

            if (int.TryParse(Console.ReadLine(), out int opcion))
            {
                return opcion;
            }
            return -1;
        }

        private static void AgregarGasto(List<Gasto> gastos, ref int siguienteId)
        {
            Console.Write("Descripción: ");
            string descripcion = Console.ReadLine();

            Console.Write("Monto: ");
            string montoInput = Console.ReadLine();

            Console.Write("Categoría: ");
            string categoria = Console.ReadLine();

            // Validación de datos
            if (string.IsNullOrWhiteSpace(descripcion) ||
                string.IsNullOrWhiteSpace(categoria) ||
                !decimal.TryParse(montoInput, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto) ||
                monto <= 0)
            {
                Console.WriteLine("Datos inválidos.");
                return;
            }

            // Crear y agregar el gasto si la validación es correcta
            Gasto nuevoGasto = new Gasto(siguienteId, descripcion, monto, categoria);
            gastos.Add(nuevoGasto);
            siguienteId++;

            Console.WriteLine("Gasto agregado.");
        }

        private static void ListarGastos(List<Gasto> gastos)
        {
            if (gastos.Count == 0)
            {
                Console.WriteLine("No hay gastos registrados.");
                return;
            }

            decimal total = 0;
            foreach (var gasto in gastos)
            {
                Console.WriteLine(gasto.ToString());
                total += gasto.Monto;
            }

            Console.WriteLine($"TOTAL GASTADO: Q {total:F2}");
        }

        private static void BuscarPorCategoria(List<Gasto> gastos)
        {
            Console.Write("Categoría a buscar: ");
            string busqueda = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(busqueda))
            {
                Console.WriteLine("Búsqueda no válida.");
                return;
            }

            bool encontrado = false;
            foreach (var gasto in gastos)
            {
                if (gasto.Categoria.IndexOf(busqueda, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Console.WriteLine($"- {gasto}");
                    encontrado = true;
                }
            }

            if (!encontrado)
            {
                Console.WriteLine("No se encontraron gastos en esa categoría.");
            }
        }

        private static void GuardarEnCsv(List<Gasto> gastos)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(ArchivoCsv))
                {
                    foreach (var gasto in gastos)
                    {
                        writer.WriteLine($"{gasto.Id},{gasto.Descripcion},{gasto.Monto.ToString(CultureInfo.InvariantCulture)},{gasto.Categoria}");
                    }
                }
                Console.WriteLine($"Gastos guardados en {ArchivoCsv} ({gastos.Count} registros).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el archivo: {ex.Message}");
            }
        }

        private static void CargarDesdeCsv(List<Gasto> gastos, ref int siguienteId)
        {
            if (!File.Exists(ArchivoCsv))
            {
                return;
            }

            try
            {
                string[] lineas = File.ReadAllLines(ArchivoCsv);
                int maxId = 0;

                foreach (string linea in lineas)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    string[] partes = linea.Split(',');
                    if (partes.Length == 4)
                    {
                        if (int.TryParse(partes[0], out int id) &&
                            decimal.TryParse(partes[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto))
                        {
                            gastos.Add(new Gasto(id, partes[1], monto, partes[3]));
                            if (id > maxId)
                            {
                                maxId = id;
                            }
                        }
                    }
                }

                siguienteId = maxId + 1; // Recalcular ID autoincremental
                Console.WriteLine($"Cargados {gastos.Count} gastos desde {ArchivoCsv}.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el archivo CSV: {ex.Message}");
            }
        }
    }
}