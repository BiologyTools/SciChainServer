using System;
using System.Globalization;
using System.Net.Http;
using SciChain;
using static SciChain.Orcid;
using static SciChain.Blockchain;
using System.Net;
using System.Runtime.CompilerServices;
namespace MyApp
{
    internal class Program
    {
        const string pass = "admin";
        static void Main(string[] args)
        {
            Wallet wallet = new Wallet();
            string dir = System.IO.Path.GetDirectoryName(Environment.ProcessPath);
            if(File.Exists(dir + "/wallet.dat"))
                wallet.Load(pass);
            else
                wallet.Save(pass);
            Initialize(wallet);
            Load();
            if (Chain.Count == 0)
                AddGenesisBlock(wallet);
            if (Chain.Count > 0)
            {
                foreach (var block in Chain)
                {
                    BlockHtml.SaveHTML(block, "../../var/www/html/" + block.Index + ".html");
                }
                int height = Chain.Count;
                do
                {
                    if (height != Chain.Count)
                    {
                        BlockHtml.SaveHTML(Chain[height - 1], "../../var/www/html/" + Chain[height - 1].Index + ".html");
                        height = Chain.Count;
                    }
                    IndexHtml.SaveHTML("../../var/www/html/index.html");
                    Thread.Sleep(10000);
                } while (true);
            }
        }
    }
    public static class BlockHtml
    {
        public static void SaveHTML(Block bl, string file)
        {
            string html = "<!DOCTYPE html><html lang=\"en\"><body>" + ToHTML(bl) + "</body></html>";
            File.WriteAllText(file, html);
        }
        public static string ToHTML(Block bl)
        {
            string html = "";
            if (bl.Index != 0)
                html += "<p><a href=\"http://www.biologytools.net/" + (bl.Index + 1) + ".html\">Next Block</a></p>";
            if(bl.Index > 0)
                html += "<p><a href=\"http://www.biologytools.net/" + (bl.Index - 1) + ".html\">Previous Block</a></p>";
            html += "<h1>Block: " + bl.Index + " Time:" + bl.TimeStamp.ToString() + " Hash: " + bl.Hash + "</h1>";
            if (bl.Transactions != null)
            {
                html += "<h2>Transactions: " + bl.Transactions.Count + "</h2>";
                foreach (Block.Transaction t in bl.Transactions)
                {
                    html += ToHTML(t);
                }
            }
            if (bl.BlockDocument != null)
            {
                html += "<h2>Document: " + bl.BlockDocument.DOI + "</h2>";
                html += "<h3>Authors</h3>";
                foreach (var item in bl.BlockDocument.Publishers)
                {
                    html += "<p>" + item + "</p>";
                }
            }
            
            return html;
        }
        public static string ToHTML(Block.Transaction t)
        {
            string html = "<p>" + t.FromAddress + " -> " + t.ToAddress + " Data:" + t.Data + " Amount: " + t.Amount + " Signature: " + t.Signature + "</p>";
            return html;
        }
    }
    public static class IndexHtml
    {
        public static void SaveHTML(string file)
        {
            int trs = 0;
            foreach (var item in Chain)
            {
                trs += item.Transactions.Count;
            }
            int users = 0;
            foreach (var item in Chain)
            {
                if(item.Transactions != null)
                foreach (var t in item.Transactions)
                {
                    if(t.TransactionType == Block.Transaction.Type.registration)
                        users++;
                }
            }
            decimal treasury = GetTreasury();
            decimal circ = Blockchain.currentSupply;
            string html = "<!DOCTYPE html><html lang=\"en\"><body>";
            html += "<p><a href=\"http://www.biologytools.net/Transactions.html\">List of All Transactions</a></p>";
            html += "<p><a href=\"http://www.biologytools.net/Pending.html\">List of Pending Transactions</a></p>";
            html += "<h1>" + "Chain Height: " + Chain.Count + "</h1>";
            html += "<p>" + "Chain Transactions: " + trs + "</p>";
            html += "<p>" + "Treasury Balance: " + treasury + "</p>";
            html += "<p>" + "Registered Users: " + users + "</p>";
            html += "<p>" + "Circulating supply: " + circ + "</p>";
            if(PendingBlocks.Count > 0)
            {
                html += "<h1>" + "Pending Blocks: " + PendingBlocks.Count + "</h1>";
                foreach (var item in PendingBlocks) 
                {
                    html += BlockHtml.ToHTML(item);
                }
            }
            html += "</body></html>";
            File.WriteAllText(file, html);
            html = "<!DOCTYPE html><html lang=\"en\"><body>";
            foreach (var item in Chain)
            {
                html += "<a href=\"http://www.biologytools.net/" + item.Index + ".html\">Block:" + item.Index + " Hash: " + item.Hash + "</a>";
                if (item.Transactions != null)
                foreach (var t in item.Transactions)
                {
                    html += BlockHtml.ToHTML(t);
                }
            }
            html += "</body></html>";
            File.WriteAllText("../../var/www/html/Transactions.html", html);

            html = "<!DOCTYPE html><html lang=\"en\"><body>";
            foreach (var item in PendingTransactions)
            {
                html += BlockHtml.ToHTML(item);
            }
            html += "</body></html>";
            File.WriteAllText("../../var/www/html/Pending.html", html);
        }
    }
}