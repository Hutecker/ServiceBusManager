using ServiceBusManager.ViewModels;
using ServiceBusManager.Views;
using System.Diagnostics;

namespace ServiceBusManager;

public partial class MainPage : ContentPage
{
    private readonly ExplorerViewModel _explorerViewModel;
    private readonly DetailsViewModel _detailsViewModel;
    private readonly LogsViewModel _logsViewModel;
    private readonly ConnectionModalViewModel _connectionModalViewModel;

    public MainPage(
        MainViewModel mainViewModel, 
        ExplorerViewModel explorerViewModel, 
        DetailsViewModel detailsViewModel,
        LogsViewModel logsViewModel,
        ConnectionModalViewModel connectionModalViewModel)
    {
        InitializeComponent();
        
        _explorerViewModel = explorerViewModel;
        _detailsViewModel = detailsViewModel;
        _logsViewModel = logsViewModel;
        _connectionModalViewModel = connectionModalViewModel;
        
        BindingContext = mainViewModel;
        
        // Log diagnostic information
        Debug.WriteLine("MainPage initialized");
        Debug.WriteLine($"MainViewModel: {mainViewModel != null}");
        Debug.WriteLine($"ExplorerViewModel: {explorerViewModel != null}");
        Debug.WriteLine($"DetailsViewModel: {detailsViewModel != null}");
        Debug.WriteLine($"LogsViewModel: {logsViewModel != null}");
        Debug.WriteLine($"ConnectionModalViewModel: {connectionModalViewModel != null}");
        
        // Find the views in the visual tree
        ApplyBindings();
    }
    
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        // Ensure bindings are applied after handler is attached
        if (Handler != null)
        {
            ApplyBindings();
        }
    }
    
    private void ApplyBindings()
    {
        // Find views
        var explorerView = FindVisualChildren<ExplorerView>(this).FirstOrDefault();
        var detailsView = FindVisualChildren<DetailsView>(this).FirstOrDefault();
        var logsView = FindVisualChildren<LogsView>(this).FirstOrDefault();
        var connectionModal = FindVisualChildren<ConnectionModal>(this).FirstOrDefault();
        
        // Debug information
        Debug.WriteLine($"Found ExplorerView: {explorerView != null}");
        Debug.WriteLine($"Found DetailsView: {detailsView != null}");
        Debug.WriteLine($"Found LogsView: {logsView != null}");
        Debug.WriteLine($"Found ConnectionModal: {connectionModal != null}");
        
        // Apply bindings if views were found
        if (explorerView != null)
        {
            explorerView.BindingContext = _explorerViewModel;
            Debug.WriteLine("Set ExplorerView.BindingContext");
        }
        
        if (detailsView != null)
        {
            detailsView.BindingContext = _detailsViewModel;
            Debug.WriteLine("Set DetailsView.BindingContext");
        }
        
        if (logsView != null)
        {
            logsView.BindingContext = _logsViewModel;
            Debug.WriteLine("Set LogsView.BindingContext");
        }
        
        if (connectionModal != null)
        {
            connectionModal.BindingContext = _connectionModalViewModel;
            Debug.WriteLine("Set ConnectionModal.BindingContext");
        }
    }
    
    // Helper method to find visual children of a specific type
    private IEnumerable<T> FindVisualChildren<T>(Element element) where T : Element
    {
        if (element is T typedElement)
        {
            yield return typedElement;
        }
        
        if (element is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is Element childElement)
                {
                    foreach (var result in FindVisualChildren<T>(childElement))
                    {
                        yield return result;
                    }
                }
            }
        }
        
        if (element is ContentView contentView && contentView.Content is Element contentElement)
        {
            foreach (var result in FindVisualChildren<T>(contentElement))
            {
                yield return result;
            }
        }
        
        if (element is Border border && border.Content is Element borderContent)
        {
            foreach (var result in FindVisualChildren<T>(borderContent))
            {
                yield return result;
            }
        }
        
        if (element is ContentPage page && page.Content is Element pageContent)
        {
            foreach (var result in FindVisualChildren<T>(pageContent))
            {
                yield return result;
            }
        }
    }
}
