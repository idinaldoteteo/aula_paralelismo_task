using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            var contas = r_Repositorio.GetContaClientes();

            var qtdePorThread = contas.Count() / 4;

            var conta_parte_1 = contas.Take(qtdePorThread);
            var conta_parte_2 = contas.Skip(qtdePorThread).Take(qtdePorThread);
            var conta_parte_3 = contas.Skip(qtdePorThread * 2).Take(qtdePorThread);
            var conta_parte_4 = contas.Skip(qtdePorThread * 3);


            var resultado = new List<string>();

            AtualizarView(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;

            Thread thread_parte_1 = new Thread(() =>
            {
                foreach (var conta in conta_parte_1)
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                }
            });

            thread_parte_1.Start();

            Thread thread_parte_2 = new Thread(() =>
            {
                foreach (var conta in conta_parte_2)
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                }
            });

            thread_parte_2.Start();

            Thread thread_parte_3 = new Thread(() =>
            {
                foreach (var conta in conta_parte_3)
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                }
            });

            thread_parte_3.Start();

            Thread thread_parte_4 = new Thread(() =>
            {
                foreach (var conta in conta_parte_4)
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                }
            });

            thread_parte_4.Start();

            while (thread_parte_1.IsAlive || thread_parte_2.IsAlive || thread_parte_3.IsAlive || thread_parte_4.IsAlive)
            {
                Thread.Sleep(250);
            }

            var fim = DateTime.Now;

            AtualizarView(resultado, fim - inicio);
        }

        private void AtualizarView(List<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}
