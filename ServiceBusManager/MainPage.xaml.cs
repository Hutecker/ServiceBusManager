using ServiceBusManager.ViewModels;

namespace ServiceBusManager;

public partial class MainPage : ContentPage
{
    private double _leftPanelWidth = 0.33;
    private double _bottomPanelHeight = 0.2;
    private const double MIN_PANEL_SIZE = 0.1;
    private const double MAX_PANEL_SIZE = 0.9;
    private bool _isDraggingHorizontal = false;
    private bool _isDraggingVertical = false;
    private Point _lastPoint;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Initialize the splitter positions
        UpdatePanelSizes();

        // Handle horizontal splitter (left/right panels)
        HorizontalSplitter.PointerPressed += (s, e) =>
        {
            var position = e.GetPosition(this);
            if (position != null)
            {
                _isDraggingHorizontal = true;
                _lastPoint = position.Value;
            }
        };

        HorizontalSplitter.PointerMoved += (s, e) =>
        {
            if (_isDraggingHorizontal)
            {
                var currentPoint = e.GetPosition(this);
                if (currentPoint != null)
                {
                    var deltaX = currentPoint.Value.X - _lastPoint.X;
                    var totalWidth = Width;
                    var newLeftWidth = _leftPanelWidth + (deltaX / totalWidth);
                    _leftPanelWidth = Math.Max(MIN_PANEL_SIZE, Math.Min(MAX_PANEL_SIZE, newLeftWidth));
                    UpdatePanelSizes();
                    _lastPoint = currentPoint.Value;
                }
            }
        };

        HorizontalSplitter.PointerReleased += (s, e) =>
        {
            _isDraggingHorizontal = false;
        };

        // Handle vertical splitter (top/bottom panels)
        VerticalSplitter.PointerPressed += (s, e) =>
        {
            var position = e.GetPosition(this);
            if (position != null)
            {
                _isDraggingVertical = true;
                _lastPoint = position.Value;
            }
        };

        VerticalSplitter.PointerMoved += (s, e) =>
        {
            if (_isDraggingVertical)
            {
                var currentPoint = e.GetPosition(this);
                if (currentPoint != null)
                {
                    var deltaY = currentPoint.Value.Y - _lastPoint.Y;
                    var totalHeight = Height;
                    var newBottomHeight = _bottomPanelHeight - (deltaY / totalHeight);
                    _bottomPanelHeight = Math.Max(MIN_PANEL_SIZE, Math.Min(MAX_PANEL_SIZE, newBottomHeight));
                    UpdatePanelSizes();
                    _lastPoint = currentPoint.Value;
                }
            }   
        };

        VerticalSplitter.PointerReleased += (s, e) =>
        {
            _isDraggingVertical = false;
        };
    }

    private void UpdatePanelSizes()
    {
        // Update column definitions
        var grid = (Grid)Content;
        grid.ColumnDefinitions[0].Width = new GridLength(_leftPanelWidth, GridUnitType.Star);
        grid.ColumnDefinitions[2].Width = new GridLength(1 - _leftPanelWidth, GridUnitType.Star);

        // Update row definitions
        grid.RowDefinitions[0].Height = new GridLength(1 - _bottomPanelHeight, GridUnitType.Star);
        grid.RowDefinitions[2].Height = new GridLength(_bottomPanelHeight, GridUnitType.Star);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdatePanelSizes();
    }
}
