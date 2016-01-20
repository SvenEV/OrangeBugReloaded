using UnityEngine;
using OrangeBugReloaded.Core;
using System.Collections.Generic;
using System;
using OrangeBugReloaded.Core.LocalSinglePlayer;

public class MapViewModel : MonoBehaviour
{
    private ILocationsProvider _locationsProvider;
    private Transform _locationsContainer;

    public LocationViewModel LocationPrefab;

    public Dictionary<Point, LocationViewModel> Locations { get; private set; }

    public ILocationsProvider LocationsProvider
    {
        get { return _locationsProvider; }
        set
        {
            if (_locationsProvider == value)
                return;

            if (_locationsProvider != null)
            {
                _locationsProvider.Locations.ItemAdded -= OnLocationAdded;
                _locationsProvider.Locations.ItemRemoved -= OnLocationRemoved;

                foreach (var locationVM in Locations)
                    locationVM.Value.Dispose();

                Destroy(_locationsContainer.gameObject);
            }

            Locations = new Dictionary<Point, LocationViewModel>();

            if (value != null)
            {
                value.Locations.ItemAdded += OnLocationAdded;
                value.Locations.ItemRemoved += OnLocationRemoved;

                foreach (var item in value.Locations)
                    OnLocationAdded(item);

                _locationsContainer = new GameObject("MapObjects").transform;
                _locationsContainer.parent = transform;
            }

            _locationsProvider = value;
        }
    }

    private void OnLocationRemoved(KeyValuePair<Point, ILocation> item)
    {
        Dispatcher.Run(() =>
        {
            var locationVM = Locations[item.Key];
            locationVM.Dispose();
            Destroy(locationVM.gameObject);
            Locations.Remove(item.Key);
        });
    }

    private void OnLocationAdded(KeyValuePair<Point, ILocation> item)
    {
        Dispatcher.Run(() =>
        {
            var location = item.Value;
            var locationVM = Instantiate(LocationPrefab);
            locationVM.transform.parent = _locationsContainer;
            locationVM.Initialize(location, this);
            Locations.Add(item.Key, locationVM);
        });
    }

    public PlayerEntityViewModel Player { get; private set; }

    public event Action PlayerFound;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save();
        }
    }

    private void Save()
    {
        // TODO: MapViewModel.Save
        if (LocationsProvider is Map)
        {
            var player = default(EntityViewModel);// _entities.Values.FirstOrDefault(o => o.Object is PlayerEntity);

            if (player != null)
            {
                //PlayerPrefs.SetInt("PlayerX", player.Object.Owner.Location.Position.X);
                //PlayerPrefs.SetInt("PlayerY", player.Object.Owner.Location.Position.Y);
            }

            (LocationsProvider as Map).SaveChangesAsync();
            Debug.Log("Map saved!");
        }
    }
}
