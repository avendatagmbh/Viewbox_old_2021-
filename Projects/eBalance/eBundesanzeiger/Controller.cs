using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace eBundesanzeiger {
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://Microsoft.ServiceModel.Samples", ConfigurationName = "ICalculator")]
    public interface ICalculator
    {

        [System.ServiceModel.OperationContractAttribute(Action = "http://Microsoft.ServiceModel.Samples/ICalculator/Add", ReplyAction = "http://Microsoft.ServiceModel.Samples/ICalculator/AddResponse")]
        double Add(double n1, double n2);

        [System.ServiceModel.OperationContractAttribute(Action = "http://Microsoft.ServiceModel.Samples/ICalculator/Subtract", ReplyAction = "http://Microsoft.ServiceModel.Samples/ICalculator/SubtractResponse")]
        double Subtract(double n1, double n2);

        [System.ServiceModel.OperationContractAttribute(Action = "http://Microsoft.ServiceModel.Samples/ICalculator/Multiply", ReplyAction = "http://Microsoft.ServiceModel.Samples/ICalculator/MultiplyResponse")]
        double Multiply(double n1, double n2);

        [System.ServiceModel.OperationContractAttribute(Action = "http://Microsoft.ServiceModel.Samples/ICalculator/Divide", ReplyAction = "http://Microsoft.ServiceModel.Samples/ICalculator/DivideResponse")]
        double Divide(double n1, double n2);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface ICalculatorChannel : ICalculator, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public class CalculatorClient : System.ServiceModel.ClientBase<ICalculator>, ICalculator
    {

        public CalculatorClient()
        {
        }

        public CalculatorClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public CalculatorClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public CalculatorClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public CalculatorClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        public double Add(double n1, double n2)
        {
            return base.Channel.Add(n1, n2);
        }

        public double Subtract(double n1, double n2)
        {
            return base.Channel.Subtract(n1, n2);
        }

        public double Multiply(double n1, double n2)
        {
            return base.Channel.Multiply(n1, n2);
        }

        public double Divide(double n1, double n2)
        {
            return base.Channel.Divide(n1, n2);
        }
    }
    
    public static class Controller
    {
        public static void Test() {
            try
            {
                CalculatorClient c = new CalculatorClient();
                var x = c.Add(5, 5);
                c.Open();
            }
            catch (Exception) {
                
            }
        }

    }
}
