import Link from "next/link";
import { Clock3, HeartPulse, ShieldCheck, Truck } from "lucide-react";

const highlights = [
  {
    icon: ShieldCheck,
    title: "Verified assortment",
    text: "We display only items available in the current catalog.",
  },
  {
    icon: Truck,
    title: "Delivery flow",
    text: "Order online and complete the checkout without extra steps.",
  },
  {
    icon: Clock3,
    title: "Order history",
    text: "Track previous purchases and repeat common orders faster.",
  },
];

export function Footer() {
  return (
    <footer className="border-t border-border/80 bg-card/70">
      <div className="container mx-auto px-4 py-10">
        <div className="grid gap-8 lg:grid-cols-[1.2fr,1fr]">
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-primary/12 text-primary">
                <HeartPulse className="h-5 w-5" />
              </div>
              <div>
                <div className="font-heading text-lg font-semibold">PharmacyApp</div>
                <div className="text-sm text-muted-foreground">
                  Pharmacy storefront focused on a clean ordering flow.
                </div>
              </div>
            </div>
            <p className="max-w-xl text-sm text-muted-foreground">
              Browse medicines, wellness products, and repeat purchases from one
              account. The storefront, cart, and order history stay in one
              consistent flow.
            </p>
          </div>

          <div className="grid gap-4 sm:grid-cols-3">
            {highlights.map(({ icon: Icon, title, text }) => (
              <div key={title} className="rounded-3xl border border-border/70 bg-background/80 p-4">
                <Icon className="mb-3 h-5 w-5 text-primary" />
                <div className="mb-1 text-sm font-semibold">{title}</div>
                <p className="text-sm text-muted-foreground">{text}</p>
              </div>
            ))}
          </div>
        </div>

        <div className="mt-8 flex flex-col gap-3 border-t border-border/70 pt-6 text-sm text-muted-foreground md:flex-row md:items-center md:justify-between">
          <p>© 2026 PharmacyApp. Online pharmacy storefront.</p>
          <nav className="flex gap-4">
            <Link href="/about" className="hover:text-foreground">
              About
            </Link>
            <Link href="/contacts" className="hover:text-foreground">
              Contacts
            </Link>
            <Link href="/privacy" className="hover:text-foreground">
              License agreement
            </Link>
          </nav>
        </div>
      </div>
    </footer>
  );
}
