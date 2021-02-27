using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentProcessor
{
    public class InvoicePaymentProcessor
    {
        private readonly InvoiceRepository _invoiceRepository;

        public InvoicePaymentProcessor(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference);

            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            var hasNoPayments = inv.Payments?.Any() != true;

            if (!hasNoPayments && inv.Amount == 0)
            {
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            if (hasNoPayments)
            {
                if (inv.Amount == 0)
                {
                    return "no payment needed";
                }

                if (payment.Amount > inv.Amount)
                {
                    return "the payment is greater than the invoice amount";
                }

                inv.AmountPaid = payment.Amount;
                inv.Payments = new List<Payment> { payment };
                if (inv.Amount == payment.Amount)
                {
                    return "invoice is now fully paid";
                }
                return "invoice is now partially paid";
            }

            var invPaymentsTotal = inv.Payments.Sum(x => x.Amount);
            var invAmountUnpaid = inv.Amount - inv.AmountPaid;

            if (invPaymentsTotal != 0)
            {
                if (inv.Amount == invPaymentsTotal)
                {
                    return "invoice was already fully paid";
                }

                if (payment.Amount > invAmountUnpaid)
                {
                    return "the payment is greater than the partial amount remaining";
                }
            }

            inv.AmountPaid += payment.Amount;
            inv.Payments.Add(payment);

            if (payment.Amount == invAmountUnpaid)
            {
                return "final partial payment received, invoice is now fully paid";
            }
            return "another partial payment received, still not fully paid";
        }
    }
}
