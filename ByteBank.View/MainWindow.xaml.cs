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
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            LimparView();

            _cts = new CancellationTokenSource();

            BtnProcessar.IsEnabled = false;
            BtnCancelar.IsEnabled = true;

            var inicio = DateTime.Now;

            var contas = r_Repositorio.GetContaClientes();

            PgsProgresso.Maximum = contas.Count();

            //Progress created by Microsoft
            var progress = new Progress<string>(str => PgsProgresso.Value++);

            //My specific option ByteBank
            //var byteBankProgress = new ByteBankProgress<string>(str => PgsProgresso.Value++);

            try
            {
                var resultado = await ConsolidarContas(contas, progress, _cts.Token);

                var fim = DateTime.Now;

                AtualizarView(resultado, fim - inicio);

            }
            catch (OperationCanceledException)
            {

                TxtTempo.Text = "Operação cancelada pelo usuário!";
            }
            finally
            {
                BtnProcessar.IsEnabled = true;
                BtnCancelar.IsEnabled = false;

            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;
            _cts.Cancel();
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = "";
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas, IProgress<string> progressBar, CancellationToken ct)
        {
            var tasks = contas.Select(conta =>
                Task.Factory.StartNew(() =>
                {
                    ct.ThrowIfCancellationRequested();

                    var resultadoConsolidado = r_Servico.ConsolidarMovimentacao(conta);

                    progressBar.Report(resultadoConsolidado);

                    ct.ThrowIfCancellationRequested();

                    return resultadoConsolidado;
                }, ct)
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
