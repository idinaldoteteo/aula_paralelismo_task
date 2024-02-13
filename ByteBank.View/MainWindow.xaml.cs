using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using ByteBank.View.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            LimparView();

            BtnProcessar.IsEnabled = false;

            var inicio = DateTime.Now;

            var contas = r_Repositorio.GetContaClientes();

            PgsProgresso.Maximum = contas.Count();

            var byteBankProgress = new ByteBankProgress<string>(str => PgsProgresso.Value++);

            var resultado = await ConsolidarContas(contas, byteBankProgress);

            var fim = DateTime.Now;

            AtualizarView(resultado, fim - inicio);

            BtnProcessar.IsEnabled = true;
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = "";
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas, IProgress<string> progressBar)
        {
            var tasks = contas.Select(conta =>
                Task.Factory.StartNew(() =>
                {
                    var resultadoConsolidado = r_Servico.ConsolidarMovimentacao(conta);

                    progressBar.Report(resultadoConsolidado);

                    return resultadoConsolidado;
                })
            );

            return await Task.WhenAll(tasks);
        }

        private void AtualizarView(IEnumerable<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{elapsedTime.Seconds}.{elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}
