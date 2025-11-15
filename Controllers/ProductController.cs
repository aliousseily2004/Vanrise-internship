using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

public class ProductController : Controller
{
    // In-memory list of products for demonstration
    private static List<Product> products = new List<Product>
    {
        new Product { Id = 1, Name = "Lenovo Laptop" },
        new Product { Id = 2, Name = "iPhone" },
        new Product { Id = 3, Name = "HP" },
        new Product { Id = 4, Name = "Samsung Galaxy" },
        new Product { Id = 5, Name = "MacBook Pro" }
    };

    // GET: Product
    public ActionResult Index()
    {
        return View(products);
    }

    // GET: Product/Filter
    public ActionResult Filter(string filter)
    {
        // If no filter is provided, return the full list
        if (string.IsNullOrEmpty(filter))
        {
            return View("Index", products);
        }

        // Filter products based on the input value (name or ID)
        var filteredProducts = products
            .Where(p => p.Name.Contains(filter))
            .ToList();

        // Pass the filtered list to the view
        return View("Index", filteredProducts);
    }

    // GET: Product/Add
    public ActionResult Add(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            // Generate a new ID for the product
            var newProduct = new Product
            {
                Id = products.Count + 1,
                Name = name
            };

            // Add the new product to the list
            products.Add(newProduct);
        }

        // Redirect to the Index action to display the updated list
        return RedirectToAction("Index");
    }

    public ActionResult Edit(int id)
    {
        // Find the product by ID
        var product = products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return HttpNotFound(); // Return 404 if product not found
        }

        // Pass the product to the Edit view
        return View(product);
    }

    // GET: Product/Update/{id}?name={name}
    public ActionResult Update(int id, string name)
    {
        // Find the product by ID
        var product = products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return RedirectToAction("Index");
        }

        // Update the product name
        product.Name = name;

        // Redirect to the Index action to display the updated list
        return RedirectToAction("Index");
    }
}
