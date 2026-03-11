using System.ComponentModel.DataAnnotations;

using GameTools.Components.Pages.NoMansSky;
using GameTools.Database;

namespace GameTools.Components.Dialogs;

public partial class EditNoMansSkyPortalAddressDialog
{
    public class ViewModel
    {
        private readonly NoMansSkyPortalAddress? _address;

        public ViewModel()
        {
            // Do nothing here
        }

        public ViewModel(NoMansSkyPortalAddress address)
        {
            _address = address;
            Name = address.Name;
            SystemName = address.SystemName;
            PlanetName = address.PlanetName;
            GalaxyId = address.GalaxyId;
            Description = address.Description;
            CoordinatesX = address.CoordinatesX;
            CoordinatesY = address.CoordinatesY;
            Address = address.Address;
        }

        public bool IsNew => _address is null;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string SystemName { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string PlanetName { get; set; } = "";

        [Required]
        public int GalaxyId { get; set; } = 1;
        public NoMansSkyGalaxy? Galaxy { get; set; }

        [Required]
        [MaxLength(65536)]
        public string Description { get; set; } = "";

        public decimal? CoordinatesX { get; set; }
        public decimal? CoordinatesY { get; set; }

        [Required]
        [MaxLength(12)]
        public string Address { get; set; } = "";

        public int? Id => _address?.Id;
    }
}