﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Connectors
{
    public class dynPortViewModel : dynViewModelBase
    {
        private readonly dynPortModel _port;
        private readonly dynNodeModel _node;

        public DelegateCommand SetCenterCommand { get; set; }
        
        public dynPortModel PortModel
        {
            get { return _port; }
        }

        public string ToolTipContent
        {
            get { return _port.ToolTipContent; }
        }

        public string PortName
        {
            get { return _port.PortName; }
        }

        public PortType PortType
        {
            get { return _port.PortType; }
        }
        
        public bool IsSelected
        {
            get { return _node.IsSelected; }
        }

        public bool IsConnected
        {
            get { return _port.IsConnected; }
        }

        public ElementState State
        {
            get { return _node.State; }    
        }

        public DelegateCommand ConnectCommand { get; set; }
        public DelegateCommand HighlightCommand { get; set; }
        public DelegateCommand UnHighlightCommand { get; set; }

        public dynPortViewModel(dynPortModel port, dynNodeModel node)
        {
            _node = node;
            _port = port;
            _port.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_port_PropertyChanged);
            _node.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_node_PropertyChanged);
            ConnectCommand = new DelegateCommand(Connect, CanConnect);
            HighlightCommand = new DelegateCommand(Highlight, CanHighlight);
            UnHighlightCommand = new DelegateCommand(UnHighlight, CanUnHighlight);
        }

        void _node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    RaisePropertyChanged("IsSelected");
                    break;
                case "State":
                    RaisePropertyChanged("State");
                    break;
            }
        }

        void _port_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ToolTipContent":
                    RaisePropertyChanged("ToolTipContent");
                    break;
                case "PortType":
                    RaisePropertyChanged("PortType");
                    break;
                case "PortName":
                    RaisePropertyChanged("PortName");
                    break;
                case "IsConnected":
                    RaisePropertyChanged("IsConnected");
                    break;
            }
            
        }

        /// <summary>
        /// Update the port model's center, triggering an update to all connectors
        /// </summary>
        /// <param name="center"></param>
        public void UpdateCenter(Point center)
        {
            _port.Center = center;
        }

        //MVVM: Obsolete method - updating of conenctors should happen automatically with bindings.
        public void Update()
        {
            foreach (dynConnectorModel c in _port.Connectors)
            {
                //calling this with null will have
                //no effect
                //c.Redraw();
            }
        }

        private void Connect()
        {
            //dynBench bench = dynSettings.Bench;

            if (!dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.IsConnecting)
            {
                //test if port already has a connection if so grab it
                //and begin connecting to somewhere else
                //don't allow the grabbing of the start connector
                if (_port.Connectors.Count > 0 && _port.Connectors[0].Start != _port)
                {
                    //create a new view model with the start referencing the 
                    //start of the connector you're about to remove
                    var c = new dynConnectorViewModel(_port.Connectors[0].Start);

                    //disconnect the connector model from its start and end ports
                    //and remove it from the connectors collection. this will also
                    //remove the view model
                    _port.Connectors[0].Kill();
                    dynSettings.Controller.DynamoViewModel.CurrentSpace.Connectors.Remove(_port.Connectors[0]);

                    //dynSettings.Controller.DynamoViewModel.ActiveConnector = _port.Connectors[0];
                    //dynSettings.Controller.DynamoViewModel.ActiveConnector.Disconnect(_port);

                    dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.ActiveConnector = c;
                    dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.IsConnecting = true;
                    
                }
                else
                {
                    try
                    {
                        //you've begun creating a connector
                        //dynConnector c = new dynConnector(_port, bench.WorkBench, e.GetPosition(bench.WorkBench));

                        //Create a connector view model to begin drawing
                        var c = new dynConnectorViewModel(_port);
                        dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.ActiveConnector = c;
                        dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.IsConnecting = true;

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.ActiveConnector.ConnectCommand.Execute(_port);
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.IsConnecting = false;
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.ActiveConnector = null;

//MVVM : Might be broken logic here. Would like to handle connection in one command
                /*
                //attempt a connection between the port
                //and the connector
                if (!dynSettings.Controller.DynamoViewModel.ActiveConnector.Connect(_port))
                {
                    dynSettings.Controller.DynamoViewModel.ActiveConnector.Kill();
                    dynSettings.Controller.DynamoViewModel.IsConnecting = false;
                    dynSettings.Controller.DynamoViewModel.ActiveConnector = null;
                }
                else
                {
                    //you've already started connecting
                    //now you're going to stop
                    dynSettings.Controller.DynamoViewModel.CurrentSpace.Connectors.Add(dynSettings.Controller.DynamoViewModel.ActiveConnector);
                    dynSettings.Controller.DynamoViewModel.IsConnecting = false;
                    dynSettings.Controller.DynamoViewModel.ActiveConnector = null;
                }
                 * */
            }
        }

        private bool CanConnect()
        {
            return true;
        }

        private void Highlight()
        {
           var connectorViewModels = dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Connectors.Where(
                x => _port.Connectors.Contains(x.ConnectorModel));

        }

        private bool CanHighlight()
        {
            return true;
        }

        private void UnHighlight()
        {
            
        }

        private bool CanUnHighlight()
        {
            return true;
        }
    }
}