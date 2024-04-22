using System;
using System.Windows.Forms;
using System;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using static System.Net.WebRequestMethods;
using System.Threading;
using System.Threading.Tasks;

//biblioteca que procesa html

namespace ejercicioArtistas
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            warning.Text = "";
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            warning.Text = "";
            conciertosList.Items.Clear();
            cancionesList.Items.Clear();

            string artista = artistaTxt.Text.ToString();
            if (artista.Length == 0)
            {
                warning.Text = "falta artista";
                conciertosList.Items.Add("Artista no encontrado");
                cancionesList.Items.Add("no existe ninguna cancion");
            }
            else {
                artista = artista.Replace(' ', '+');
                string url = "https://www.setlist.fm/search?query=" + artista;
                var httpClient = new HttpClient();
                var html = httpClient.GetStringAsync(url).Result;

                var htmlDocument = new HtmlAgilityPack.HtmlDocument();  //**
                htmlDocument.LoadHtml(html);


               
                conciertos = GetConcert(htmlDocument);
                GetAllRestConcerts(htmlDocument, artista);
            }
        }
        List<(string, string)> conciertos;



    
    public List<(string, string)> GetConcert(HtmlAgilityPack.HtmlDocument doc)
    {
        HtmlNodeCollection Nodconc = doc.DocumentNode.SelectNodes("//h2/a");
        List<(string, string)> concert = new List<(string, string)>();

        if (Nodconc != null)
        {
            foreach (HtmlNode node in Nodconc)
            {
                string urlSongs = node.Attributes["href"].Value;
                if (urlSongs.EndsWith(".html")) {
                    concert.Add((node.InnerText, urlSongs));
                    conciertosList.Items.Add(node.InnerText);
                }
            }
        }
        else
            warning.Text = "No encontre conciertos";

        return concert;
    }
    public async void GetAllRestConcerts(HtmlAgilityPack.HtmlDocument doc, string artista) {
        HtmlNodeCollection Indices = doc.DocumentNode.SelectNodes("//a[@title='Go to last page']");
            if (Indices != null)
            { //existe un item 
                foreach (var item in Indices) {
                    int lastP = int.Parse(item.InnerText);

                    for (int i = 2; i <= lastP; i++)
                    { 
                        string url  = "https://www.setlist.fm/search?page=" + i + "&query=" + artista;

                        var httpClient = new HttpClient();
                        var html = httpClient.GetStringAsync(url).Result;
                        var htmlDocument = new HtmlAgilityPack.HtmlDocument();  //**
                        htmlDocument.LoadHtml(html);

                        AddConcerts(htmlDocument);

                        await Task.Delay(1500);

                        //Thread.Sleep(1500);
                    }                
                }
            }
            else {
                MessageBox.Show("no encontro nada");
                return;
            }
    }
        public void AddConcerts(HtmlAgilityPack.HtmlDocument doc)
        {
            HtmlNodeCollection Nodconc = doc.DocumentNode.SelectNodes("//h2/a");

            if (Nodconc != null)
            {
                foreach (HtmlNode node in Nodconc)
                {
                    string urlSongs = node.Attributes["href"].Value;
                    if (urlSongs.EndsWith(".html"))
                    {
                        conciertos.Add((node.InnerText, urlSongs));
                        conciertosList.Items.Add(node.InnerText);
                    }
                }
            }
            else
                warning.Text = "No encontre conciertos";
        }



        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //cuando se van a hacer varias peticiones seguidas pon un sleep o delay 150 ms, por que 
        //la pagina se bloquea ya que se hacen muchas peticiones seguidas

        private void conciertosList_SelectedIndexChanged(object sender, EventArgs e)
        {
            warning.Text = "";
            cancionesList.Items.Clear();

            int index = conciertosList.SelectedIndex;

            songs = songswebPet(index);

            int aux = 0;
            foreach (var item in songs)
            {
                aux++;
                cancionesList.Items.Add($"{aux}. {item}");
            }
        }

        private List<string> songswebPet(int index)
        {
            string canUrl = conciertos[index].Item2;
            string url = "https://www.setlist.fm/" + canUrl;


            var httpClient = new HttpClient();
            var html = httpClient.GetStringAsync(url).Result;
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();  //**
            htmlDocument.LoadHtml(html);

            songs = GetListSongs(htmlDocument);

            return songs;
        }

        List<string> songs = new List<string>();
        public List<string> GetListSongs(HtmlAgilityPack.HtmlDocument doc)
        {
            songs.Clear();
            HtmlNodeCollection Nodconc = doc.DocumentNode.SelectNodes("//a[@class='songLabel']");

            if (Nodconc != null)
            {
                foreach (HtmlNode node in Nodconc)
                {
                    songs.Add(node.InnerText);
                }
            }
            else
            {
                warning.Text = "No encontre canciones";
                return songs;
            }

            return songs;
        }
    }
}
