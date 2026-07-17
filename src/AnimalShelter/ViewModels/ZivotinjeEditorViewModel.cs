using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnimalShelter.Models.Enums;

namespace AnimalShelter.ViewModels;

public partial class ZivotinjeEditorViewModel : ObservableObject
{
    private readonly int _udruzenjeId;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _naziv = string.Empty;

    [ObservableProperty]
    private KategorijaZivotinje _kategorija;

    [ObservableProperty]
    private int? _starost;

    [ObservableProperty]
    private PolZivotinje _pol;

    [ObservableProperty]
    private string? _rasa;

    [ObservableProperty]
    private ZdravljeZivotinje _zdravstvenoStanje;

    [ObservableProperty]
    private string? _opis;

    [ObservableProperty]
    private StatusZivotinje _status;

    [ObservableProperty]
    private string _slike = string.Empty;   // CSV za unos

    [ObservableProperty]
    private string _video = string.Empty;   // CSV za unos

    [ObservableProperty]
    private int? _udomiteljId;

    [ObservableProperty]
    private int? _volonterId;

    [ObservableProperty]
    private bool _isEditMode;

    public IAsyncRelayCommand? SacuvajCommand { get; }
    public IRelayCommand? OdustaniCommand { get; }

    public event EventHandler? RequestClose;
    
    public ObservableCollection<KategorijaZivotinje> KategorijaList { get; } = new();
    public ObservableCollection<PolZivotinje> PolList { get; } = new();
    public ObservableCollection<ZdravljeZivotinje> ZdravstvenoStanjeList { get; } = new();
    public ObservableCollection<StatusZivotinje> StatusList { get; } = new();


    public ZivotinjeEditorViewModel(int udruzenjeId)
    {
        _udruzenjeId = udruzenjeId;

        // Napuni liste
        foreach (KategorijaZivotinje k in Enum.GetValues(typeof(KategorijaZivotinje)))
            KategorijaList.Add(k);
        foreach (PolZivotinje p in Enum.GetValues(typeof(PolZivotinje)))
            PolList.Add(p);
        foreach (ZdravljeZivotinje z in Enum.GetValues(typeof(ZdravljeZivotinje)))
            ZdravstvenoStanjeList.Add(z);
        foreach (StatusZivotinje s in Enum.GetValues(typeof(StatusZivotinje)))
            StatusList.Add(s);

        // Default vrijednosti
        Kategorija = KategorijaZivotinje.PAS;
        Pol = PolZivotinje.NEPOZNATO;
        ZdravstvenoStanje = ZdravljeZivotinje.ZDRAVA;
        Status = StatusZivotinje.DOSTUPNA;

        SacuvajCommand = new AsyncRelayCommand(SacuvajAsync, CanSacuvaj);
        OdustaniCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
    }

    private bool CanSacuvaj()
    {
        // Obavezna polja: Naziv, Kategorija, Pol, ZdravstvenoStanje, Status
        return !string.IsNullOrWhiteSpace(Naziv);
    }

    private async Task SacuvajAsync()
    {
        if (!CanSacuvaj()) return;
        RequestClose?.Invoke(this, new DialogResultEventArgs(true));
        await Task.CompletedTask;
    }

    public class DialogResultEventArgs : EventArgs
    {
        public bool Result { get; }
        public DialogResultEventArgs(bool result) => Result = result;
    }
}