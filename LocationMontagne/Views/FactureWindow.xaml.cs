using LocationMontagne.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using iText.Kernel.Colors;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using Border = iText.Layout.Borders.Border;
using System.IO;
using iText.Kernel.Geom;
using System.Windows.Media;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Properties;

namespace LocationMontagne.Views
{
    /// <summary>
    /// Logique d'interaction pour FactureWindow.xaml
    /// </summary>
    public partial class FactureWindow : Window
    {
        private readonly Location _location;
        private readonly LocationArticle _locationArticle;

        /// <summary>
        /// Initialise une nouvelle instance de la fenêtre de facture
        /// avec la location et l'article de location spécifiés.
        /// </summary>
        /// <param name="location">La location associée à la facture</param>
        /// <param name="locationArticle">L'article loué</param>
        public FactureWindow(Location location, LocationArticle locationArticle)
        {
            InitializeComponent();
            _location = location;
            _locationArticle = locationArticle;
            LoadFactureData();
        }

        /// <summary>
        /// Charge les données de la facture dans l'interface utilisateur.
        /// Affiche les informations client, les détails de facturation et les articles loués.
        /// </summary>
        private void LoadFactureData()
        {
            // Infos client
            ClientNameText.Text = $"{_location.user.nom} {_location.user.prenom}";
            ClientAddressText.Text = _location.user.adresse;
            ClientCityText.Text = $"{_location.user.ville.codePostal} {_location.user.ville.nomVille}";

            // Infos facture
            FactureNumberText.Text = $"N° {_location.facture.idFacture}";
            FactureDateText.Text = $"Date : {_location.facture.dateFacture:dd/MM/yyyy}";

            // montant HT
            decimal montantHT = Math.Round(_location.facture.montant / 1.2m, 2);
            decimal montantTVA = _location.facture.montant - montantHT;

            // Ajout article
            var articleGrid = new Grid();
            articleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            articleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            articleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            articleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

            // alternance couleur
            articleGrid.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F8F9FA");

            // Nb jour
            int nombreJours = (_location.dateFinLocation - _location.dateDebutLocation).Days + 1;
            decimal prixUnitaire = _locationArticle.article.tarif;

            // Description
            var descriptionText = new TextBlock
            {
                Text = $"{_locationArticle.article.nomArticle}",
                Style = (System.Windows.Style)FindResource("TableCellStyle"),
                FontWeight = System.Windows.FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(descriptionText, 0);

            // Sous-description - période
            var periodText = new TextBlock
            {
                Text = $"Période: {_location.dateDebutLocation:dd/MM/yyyy} au {_location.dateFinLocation:dd/MM/yyyy} ({nombreJours} jour{(nombreJours > 1 ? "s" : "")})",
                Foreground = (SolidColorBrush)FindResource("TextSecondaryColor"),
                FontSize = 12,
                Margin = new Thickness(12, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };

            // Combiner desc et sous desc
            var descriptionStack = new StackPanel();
            descriptionStack.Children.Add(descriptionText);
            descriptionStack.Children.Add(periodText);
            Grid.SetColumn(descriptionStack, 0);

            // Quantité
            var quantiteText = new TextBlock
            {
                Text = _locationArticle.quantite.ToString(),
                Style = (System.Windows.Style)FindResource("TableCellStyle"),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            Grid.SetColumn(quantiteText, 1);

            // Prix unitaire
            var prixText = new TextBlock
            {
                Text = $"{prixUnitaire:C2}",
                Style = (System.Windows.Style)FindResource("TableCellNumericStyle")
            };
            Grid.SetColumn(prixText, 2);

            // Prix total
            var totalText = new TextBlock
            {
                Text = $"{_location.facture.montant:C2}",
                Style = (System.Windows.Style)FindResource("TableCellNumericStyle"),
                FontWeight = System.Windows.FontWeights.SemiBold
            };
            Grid.SetColumn(totalText, 3);

            // Ajout des éléments
            articleGrid.Children.Add(descriptionStack);
            articleGrid.Children.Add(quantiteText);
            articleGrid.Children.Add(prixText);
            articleGrid.Children.Add(totalText);

            ArticlesPanel.Children.Add(articleGrid);

            // MAJ totaux
            SubTotalText.Text = $"{montantHT:C2}";
            TaxText.Text = $"{montantTVA:C2}";
            TotalText.Text = $"{_location.facture.montant:C2}";

            if(_location.etatLocation.nomEtatLocation == "Annulée")
            {
                TotalText.Visibility = Visibility.Collapsed;
                TaxText.Visibility = Visibility.Collapsed;
                SubTotalText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton "Exporter en PDF".
        /// Génère un document PDF formaté avec toutes les informations de la facture
        /// et propose à l'utilisateur de l'enregistrer.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] pdfBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var writer = new PdfWriter(ms))
                    {
                        using (var pdf = new PdfDocument(writer))
                        {
                            var document = new Document(pdf, PageSize.A4);
                            document.SetMargins(40, 40, 40, 40);

                            // Définition des polices et des couleurs pour correspondre aux styles XAML
                            PdfFont fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                            PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                            // Couleurs tirées de App.xaml
                            DeviceRgb primaryColor = new DeviceRgb(30, 95, 116); // #1E5F74
                            DeviceRgb secondaryColor = new DeviceRgb(255, 140, 66); // #FF8C42
                            DeviceRgb textSecondaryColor = new DeviceRgb(108, 117, 125); // #6C757D
                            DeviceRgb backgroundColor = new DeviceRgb(248, 249, 250); // #F8F9FA
                            DeviceRgb borderColor = new DeviceRgb(222, 226, 230); // #DEE2E6

                            // En-tête avec logo et informations
                            Table headerTable = new Table(2).UseAllAvailableWidth();

                            // Logo et nom entreprise (colonne gauche)
                            Cell logoCell = new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetPaddingBottom(20);

                            Paragraph logoText = new Paragraph("🏔️ LOCATION MONTAGNE")
                                .SetFontSize(20)
                                .SetFont(fontBold)
                                .SetFontColor(primaryColor);
                            logoCell.Add(logoText);

                            Paragraph addressText = new Paragraph("91300 Massy, France")
                                .SetFontSize(10)
                                .SetFontColor(textSecondaryColor)
                                .SetMarginTop(8);
                            logoCell.Add(addressText);

                            Paragraph emailText = new Paragraph("contact@location-montagne.fr")
                                .SetFontSize(10)
                                .SetFontColor(textSecondaryColor);
                            logoCell.Add(emailText);

                            // Informations facture (colonne droite)
                            Cell factureCell = new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetPaddingBottom(20);

                            Paragraph factureTitle = new Paragraph("FACTURE")
                                .SetFontSize(20)
                                .SetFont(fontBold)
                                .SetMarginBottom(8);
                            factureCell.Add(factureTitle);

                            Paragraph factureNumber = new Paragraph($"N° {_location.facture.idFacture}")
                                .SetFontSize(11)
                                .SetMarginBottom(4);
                            factureCell.Add(factureNumber);

                            Paragraph factureDate = new Paragraph($"Date : {_location.facture.dateFacture:dd/MM/yyyy}")
                                .SetFontSize(11);
                            factureCell.Add(factureDate);

                            headerTable.AddCell(logoCell);
                            headerTable.AddCell(factureCell);
                            document.Add(headerTable);

                            // Séparateur
                            LineSeparator headerSeparator = new LineSeparator(new SolidLine(1f))
                                .SetMarginBottom(20)
                                .SetMarginTop(5);
                            document.Add(headerSeparator);

                            // Informations client et paiement
                            Table clientTable = new Table(2).UseAllAvailableWidth()
                                .SetMarginBottom(30);

                            // Client (colonne gauche)
                            Cell clientInfoCell = new Cell()
                                .SetBorder(Border.NO_BORDER);

                            clientInfoCell.Add(new Paragraph("Facturé à :")
                                .SetFont(fontBold)
                                .SetMarginBottom(8));

                            clientInfoCell.Add(new Paragraph($"{_location.user.nom} {_location.user.prenom}")
                                .SetFontSize(12)
                                .SetFont(fontBold)
                                .SetMarginBottom(4));

                            clientInfoCell.Add(new Paragraph(_location.user.adresse)
                                .SetMarginBottom(4));

                            clientInfoCell.Add(new Paragraph($"{_location.user.ville.codePostal} {_location.user.ville.nomVille}"));

                            // Paiement (colonne droite)
                            Cell paymentCell = new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.RIGHT);

                            paymentCell.Add(new Paragraph("Méthode de paiement :")
                                .SetFont(fontBold)
                                .SetMarginBottom(8));

                            paymentCell.Add(new Paragraph("Paiement en ligne"));

                            clientTable.AddCell(clientInfoCell);
                            clientTable.AddCell(paymentCell);
                            document.Add(clientTable);

                            // Titre détails de la location
                            Paragraph detailsTitle = new Paragraph("Détails de la location")
                                .SetFontSize(16)
                                .SetFont(fontBold)
                                .SetMarginBottom(16);
                            document.Add(detailsTitle);

                            // Tableau des articles
                            Table articleTable = new Table(UnitValue.CreatePercentArray(new float[] { 40, 20, 20, 20 }))
                                .UseAllAvailableWidth()
                                .SetMarginBottom(30);

                            // En-têtes du tableau
                            Cell descHeader = new Cell()
                                .SetBackgroundColor(primaryColor)
                                .SetPadding(10)
                                .Add(new Paragraph("Description")
                                    .SetFontColor(ColorConstants.WHITE)
                                    .SetFont(fontBold));

                            Cell qtyHeader = new Cell()
                                .SetBackgroundColor(primaryColor)
                                .SetPadding(10)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Paragraph("Quantité")
                                    .SetFontColor(ColorConstants.WHITE)
                                    .SetFont(fontBold));

                            Cell priceHeader = new Cell()
                                .SetBackgroundColor(primaryColor)
                                .SetPadding(10)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .Add(new Paragraph("Prix unitaire")
                                    .SetFontColor(ColorConstants.WHITE)
                                    .SetFont(fontBold));

                            Cell totalHeader = new Cell()
                                .SetBackgroundColor(primaryColor)
                                .SetPadding(10)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .Add(new Paragraph("Total")
                                    .SetFontColor(ColorConstants.WHITE)
                                    .SetFont(fontBold));

                            articleTable.AddHeaderCell(descHeader);
                            articleTable.AddHeaderCell(qtyHeader);
                            articleTable.AddHeaderCell(priceHeader);
                            articleTable.AddHeaderCell(totalHeader);

                            // Contenu du tableau
                            int nombreJours = (_location.dateFinLocation - _location.dateDebutLocation).Days + 1;
                            decimal prixUnitaire = _locationArticle.article.tarif;

                            // Description
                            Cell descCell = new Cell()
                                .SetBackgroundColor(backgroundColor)
                                .SetPadding(10)
                                .SetBorder(Border.NO_BORDER);

                            Paragraph articleName = new Paragraph(_locationArticle.article.nomArticle)
                                .SetFont(fontBold)
                                .SetMarginBottom(5);
                            descCell.Add(articleName);

                            Paragraph periodText = new Paragraph($"Période: {_location.dateDebutLocation:dd/MM/yyyy} au {_location.dateFinLocation:dd/MM/yyyy} ({nombreJours} jour{(nombreJours > 1 ? "s" : "")})")
                                .SetFontSize(10)
                                .SetFontColor(textSecondaryColor);
                            descCell.Add(periodText);

                            // Quantité
                            Cell qtyCell = new Cell()
                                .SetBackgroundColor(backgroundColor)
                                .SetPadding(10)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .SetBorder(Border.NO_BORDER)
                                .Add(new Paragraph(_locationArticle.quantite.ToString()));

                            // Prix unitaire
                            Cell priceCell = new Cell()
                                .SetBackgroundColor(backgroundColor)
                                .SetPadding(10)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .SetBorder(Border.NO_BORDER)
                                .Add(new Paragraph($"{prixUnitaire:C2}"));

                            // Total
                            Cell totalCell = new Cell()
                                .SetBackgroundColor(backgroundColor)
                                .SetPadding(10)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                                .SetBorder(Border.NO_BORDER)
                                .Add(new Paragraph($"{_location.facture.montant:C2}")
                                    .SetFont(fontBold));

                            articleTable.AddCell(descCell);
                            articleTable.AddCell(qtyCell);
                            articleTable.AddCell(priceCell);
                            articleTable.AddCell(totalCell);

                            document.Add(articleTable);

                            // montant HT et calculs
                            decimal montantHT = Math.Round(_location.facture.montant / 1.2m, 2);
                            decimal montantTVA = _location.facture.montant - montantHT;

                            // Tableau des totaux
                            Table totalsTable = new Table(2)
                                .SetWidth(240)
                                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.RIGHT);

                            // Sous-total
                            Cell subtotalLabelCell = new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetPadding(5)
                                .Add(new Paragraph("Sous-total :"));

                            Cell subtotalValueCell = new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetPadding(5)
                                .Add(new Paragraph($"{montantHT:C2}"));

                            totalsTable.AddCell(subtotalLabelCell);
                            totalsTable.AddCell(subtotalValueCell);

                            // TVA
                            Cell taxLabelCell = new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetPadding(5)
                                .Add(new Paragraph("TVA (20%) :"));

                            Cell taxValueCell = new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetPadding(5)
                                .Add(new Paragraph($"{montantTVA:C2}"));

                            totalsTable.AddCell(taxLabelCell);
                            totalsTable.AddCell(taxValueCell);

                            // Séparateur dans le tableau des totaux
                            Cell separatorCell = new Cell(1, 2)
                                .SetBorder(Border.NO_BORDER)
                                .SetPadding(0);

                            LineSeparator totalSeparator = new LineSeparator(new SolidLine(1f))
                                .SetMarginTop(8)
                                .SetMarginBottom(8);
                            separatorCell.Add(totalSeparator);
                            totalsTable.AddCell(separatorCell);

                            // Total TTC
                            Cell totalLabelCell = new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetPadding(5)
                                .Add(new Paragraph("Total TTC :")
                                    .SetFont(fontBold));

                            Cell totalValueCell = new Cell()
                                .SetBorder(Border.NO_BORDER)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetPadding(5)
                                .Add(new Paragraph($"{_location.facture.montant:C2}")
                                    .SetFont(fontBold)
                                    .SetFontSize(16)
                                    .SetFontColor(primaryColor));

                            totalsTable.AddCell(totalLabelCell);
                            totalsTable.AddCell(totalValueCell);

                            document.Add(totalsTable);

                            // Notes et conditions dans une boîte de fond gris
                            Table notesTable = new Table(1)
                                .UseAllAvailableWidth()
                                .SetMarginTop(30);

                            Cell notesCell = new Cell()
                                .SetBackgroundColor(backgroundColor)
                                .SetPadding(16);

                            notesCell.Add(new Paragraph("Notes")
                                .SetFont(fontBold)
                                .SetMarginBottom(8));

                            notesCell.Add(new Paragraph("Merci d'avoir choisi Location Montagne pour vos besoins en équipement de montagne. En cas de questions concernant cette facture, n'hésitez pas à nous contacter.")
                                .SetFontSize(11));

                            notesTable.AddCell(notesCell);
                            document.Add(notesTable);

                            // Pied de page
                            Paragraph footer = new Paragraph("Location Montagne - Facture générée le " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(9)
                                .SetFontColor(textSecondaryColor)
                                .SetMarginTop(30);
                            document.Add(footer);

                            document.Close();
                        }
                    }

                    // Récup du PDF en bytes
                    pdfBytes = ms.ToArray();
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Fichier PDF (*.pdf)|*.pdf",
                    FileName = $"Facture_{_location.facture.idFacture}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, pdfBytes);
                    MessageBox.Show("La facture a été exportée avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'export du PDF : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Gère le clic sur le bouton "Fermer".
        /// Ferme la fenêtre de facture.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
