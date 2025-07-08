using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumBookstore
{
    public abstract class Book
    {
        public string ISBN { get; }
        public string Title { get; }
        public string Author { get; }
        public int PublicationYear { get; }
        public decimal Price { get; }

        protected Book(string isbn, string title, string author, int publicationYear, decimal price)
        {
            if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("ISBN cannot be empty");
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty");
            if (string.IsNullOrWhiteSpace(author)) throw new ArgumentException("Author cannot be empty");
            if (publicationYear > DateTime.Now.Year) throw new ArgumentException("Invalid publication year");
            if (price < 0) throw new ArgumentException("Price cannot be negative");

            ISBN = isbn;
            Title = title;
            Author = author;
            PublicationYear = publicationYear;
            Price = price;
        }

        public abstract void Purchase(int quantity, string customerEmail, string shippingAddress);
    }

    public class PaperBook : Book
    {
        public int StockQuantity { get; private set; }

        public PaperBook(string isbn, string title, string author, int publicationYear, decimal price, int stockQuantity)
            : base(isbn, title, author, publicationYear, price)
        {
            if (stockQuantity < 0) throw new ArgumentException("Stock quantity cannot be negative");
            StockQuantity = stockQuantity;
        }

        public override void Purchase(int quantity, string customerEmail, string shippingAddress)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
            if (quantity > StockQuantity) throw new InvalidOperationException("Insufficient stock available");
            if (string.IsNullOrWhiteSpace(shippingAddress)) throw new ArgumentException("Shipping address is required");

            StockQuantity -= quantity;
            Console.WriteLine($"Quantum book store: Processed payment of {Price * quantity:C} for {Title}");
            ShippingService.ShipToAddress(shippingAddress);
        }
    }

    public class EBook : Book
    {
        public string FileFormat { get; }

        public EBook(string isbn, string title, string author, int publicationYear, decimal price, string fileFormat)
            : base(isbn, title, author, publicationYear, price)
        {
            if (string.IsNullOrWhiteSpace(fileFormat)) throw new ArgumentException("File format must be specified");
            FileFormat = fileFormat;
        }

        public override void Purchase(int quantity, string customerEmail, string shippingAddress)
        {
            if (quantity != 1) throw new InvalidOperationException("Only single eBook purchases are allowed");
            if (string.IsNullOrWhiteSpace(customerEmail)) throw new ArgumentException("Customer email is required");

            Console.WriteLine($"Quantum book store: Processed payment of {Price:C} for {Title}");
            MailService.SendToEmail(customerEmail, FileFormat);
        }
    }

    public class ShowcaseBook : Book
    {
        public ShowcaseBook(string isbn, string title, string author, int publicationYear, decimal price)
            : base(isbn, title, author, publicationYear, price)
        {
        }

        public override void Purchase(int quantity, string customerEmail, string shippingAddress)
        {
            throw new InvalidOperationException("Showcase books are not available for purchase");
        }
    }

    public static class ShippingService
    {
        public static void ShipToAddress(string address)
        {
            Console.WriteLine($"Quantum book store: Shipping physical copy to {address}");
        }
    }

    public static class MailService
    {
        public static void SendToEmail(string email, string fileFormat)
        {
            Console.WriteLine($"Quantum book store: Delivering {fileFormat} format eBook to {email}");
        }
    }

    public class InventoryManager
    {
        private readonly Dictionary<string, Book> _inventory = new Dictionary<string, Book>();

        public void AddBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (_inventory.ContainsKey(book.ISBN)) throw new InvalidOperationException("Book with this ISBN already exists");

            _inventory.Add(book.ISBN, book);
            Console.WriteLine($"Quantum book store: Added '{book.Title}' to inventory");
        }

        public IReadOnlyCollection<Book> RemoveOutdatedBooks(int maxAgeYears)
        {
            if (maxAgeYears <= 0) throw new ArgumentException("Maximum age must be positive");

            var currentYear = DateTime.Now.Year;
            var outdatedBooks = _inventory.Values
                .Where(b => currentYear - b.PublicationYear > maxAgeYears)
                .ToList();

            foreach (var book in outdatedBooks)
            {
                _inventory.Remove(book.ISBN);
                Console.WriteLine($"Quantum book store: Removed outdated book '{book.Title}' (published {book.PublicationYear})");
            }

            return outdatedBooks.AsReadOnly();
        }

        public decimal ProcessPurchase(string isbn, int quantity, string customerEmail, string shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("ISBN is required");
            if (!_inventory.TryGetValue(isbn, out var book)) throw new KeyNotFoundException("Book not found in inventory");

            book.Purchase(quantity, customerEmail, shippingAddress);
            return book.Price * quantity;
        }

        public Book GetBookDetails(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("ISBN is required");
            return _inventory.TryGetValue(isbn, out var book) ? book : null;
        }
    }

    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Quantum book store: System initialization complete");

            var inventory = new InventoryManager();

            // Adding some additional books for demonstration
            inventory.AddBook(new EBook("1234", "Math", "Mohamed Ayman", 2025, 1000, "PDF"));

            var isRunning = true;

            while (isRunning)
            {
                Console.WriteLine("\nQuantum book store: Main Menu");
                Console.WriteLine("1. Add new book to inventory");
                Console.WriteLine("2. Purchase book");
                Console.WriteLine("3. Remove outdated books");
                Console.WriteLine("4. View book details");
                Console.WriteLine("5. Exit");

                Console.Write("Quantum book store: Enter your choice: ");
                var input = Console.ReadLine();

                try
                {
                    switch (input)
                    {
                        case "1":
                            AddBookMenu(inventory);
                            break;
                        case "2":
                            PurchaseBookMenu(inventory);
                            break;
                        case "3":
                            RemoveOutdatedBooksMenu(inventory);
                            break;
                        case "4":
                            ViewBookDetailsMenu(inventory);
                            break;
                        case "5":
                            isRunning = false;
                            break;
                        default:
                            Console.WriteLine("Quantum book store: Invalid option selected");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Quantum book store: Operation failed - {ex.Message}");
                }
            }

            Console.WriteLine("Quantum book store: System shutdown complete");
        }

        private static void AddBookMenu(InventoryManager inventory)
        {
            Console.WriteLine("\nQuantum book store: Add New Book");

            Console.Write("Enter book type (paper/ebook/showcase): ");
            var type = Console.ReadLine()?.Trim().ToLower();

            Console.Write("Enter ISBN: ");
            var isbn = Console.ReadLine();

            Console.Write("Enter title: ");
            var title = Console.ReadLine();

            Console.Write("Enter author: ");
            var author = Console.ReadLine();

            Console.Write("Enter publication year: ");
            var year = int.Parse(Console.ReadLine());

            Console.Write("Enter price: ");
            var price = decimal.Parse(Console.ReadLine());

            Book book;
            switch (type)
            {
                case "paper":
                    Console.Write("Enter stock quantity: ");
                    var stock = int.Parse(Console.ReadLine());
                    book = new PaperBook(isbn, title, author, year, price, stock);
                    break;
                case "ebook":
                    Console.Write("Enter file format: ");
                    var format = Console.ReadLine();
                    book = new EBook(isbn, title, author, year, price, format);
                    break;
                case "showcase":
                    book = new ShowcaseBook(isbn, title, author, year, price);
                    break;
                default:
                    throw new ArgumentException("Invalid book type specified");
            }

            inventory.AddBook(book);
        }

        private static void PurchaseBookMenu(InventoryManager inventory)
        {
            Console.WriteLine("\nQuantum book store: Purchase Book");

            Console.Write("Enter ISBN: ");
            var isbn = Console.ReadLine();

            Console.Write("Enter quantity: ");
            var quantity = int.Parse(Console.ReadLine());

            var book = inventory.GetBookDetails(isbn);
            string email = null;
            string address = null;

            if (book is EBook)
            {
                Console.Write("Enter customer email: ");
                email = Console.ReadLine();
            }
            else if (book is PaperBook)
            {
                Console.Write("Enter shipping address: ");
                address = Console.ReadLine();
            }

            var total = inventory.ProcessPurchase(isbn, quantity, email, address);
            Console.WriteLine($"Quantum book store: Purchase completed. Total: {total:C}");
        }

        private static void RemoveOutdatedBooksMenu(InventoryManager inventory)
        {
            Console.WriteLine("\nQuantum book store: Remove Outdated Books");

            Console.Write("Enter maximum age (years): ");
            var maxAge = int.Parse(Console.ReadLine());

            var removed = inventory.RemoveOutdatedBooks(maxAge);
            Console.WriteLine($"Quantum book store: Removed {removed.Count} outdated books");
        }

        private static void ViewBookDetailsMenu(InventoryManager inventory)
        {
            Console.WriteLine("\nQuantum book store: View Book Details");

            Console.Write("Enter ISBN: ");
            var isbn = Console.ReadLine();

            var book = inventory.GetBookDetails(isbn);
            if (book == null)
            {
                Console.WriteLine("Quantum book store: Book not found");
                return;
            }

            Console.WriteLine($"Title: {book.Title}");
            Console.WriteLine($"Author: {book.Author}");
            Console.WriteLine($"Year: {book.PublicationYear}");
            Console.WriteLine($"Price: {book.Price:C}");

            switch (book)
            {
                case PaperBook pb:
                    Console.WriteLine($"Type: Paper Book");
                    Console.WriteLine($"Stock: {pb.StockQuantity}");
                    break;
                case EBook eb:
                    Console.WriteLine($"Type: EBook ({eb.FileFormat})");
                    break;
                case ShowcaseBook _:
                    Console.WriteLine("Type: Showcase Book (Not for sale)");
                    break;
            }
        }
    }
}