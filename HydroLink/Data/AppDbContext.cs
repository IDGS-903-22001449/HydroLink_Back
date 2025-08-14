using HydroLink.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace HydroLink.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuario { get; set; } = default!;
        public DbSet<Producto> Producto { get; set; } = default!;
        public DbSet<MateriaPrima> MateriaPrima { get; set; } = default!;
        public DbSet<Proveedor> Proveedor { get; set; } = default!;
        public DbSet<Compra> Compra { get; set; } = default!;
        public DbSet<CompraDetalle> CompraDetalle { get; set; } = default!;
        public DbSet<Venta> Venta { get; set; } = default!;
        public DbSet<VentaDetalle> VentaDetalle { get; set; } = default!;
        public DbSet<Cotizacion> Cotizacion { get; set; } = default!;
        public DbSet<CotizacionDetalle> CotizacionDetalle { get; set; } = default!;
        public DbSet<ListaMaterial> ListaMaterial { get; set; } = default!;
        public DbSet<Comentario> Comentario { get; set; } = default!;
        public DbSet<Persona> Persona { get; set; } = default!;
        public DbSet<Cliente> Cliente { get; set; } = default!;
        public DbSet<CostoPromedioMateriaPrima> CostoPromedioMateriaPrima { get; set; } = default!;
        public DbSet<MovimientoInventario> MovimientoInventario { get; set; } = default!;
        public DbSet<LoteInventario> LoteInventario { get; set; } = default!;
        public DbSet<Componente> Componente { get; set; } = default!;
        public DbSet<ProductoModular> ProductoModular { get; set; } = default!;
        public DbSet<ComponenteModulo> ComponenteModulo { get; set; } = default!;
        public DbSet<CotizacionModular> CotizacionModular { get; set; } = default!;
        public DbSet<CotizacionModularDetalle> CotizacionModularDetalle { get; set; } = default!;
        public DbSet<ComponenteMateriaPrima> ComponenteMateriaPrima { get; set; } = default!;
        public DbSet<ProductoHydroLink> ProductoHydroLink { get; set; } = default!;
        public DbSet<ComponenteRequerido> ComponenteRequerido { get; set; } = default!;
        public DbSet<MovimientoComponente> MovimientoComponente { get; set; } = default!;
        public DbSet<ProductoComprado> ProductoComprado { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Persona>()
                .HasDiscriminator<string>("TipoPersona")
                .HasValue<Cliente>("Cliente")
                .HasValue<Proveedor>("Proveedor");

            builder.Entity<Cliente>()
                .Property(c => c.FechaRegistro)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Cliente>()
                .Property(c => c.Activo)
                .HasDefaultValue(true);

            builder.Entity<CompraDetalle>().Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            builder.Entity<ListaMaterial>().Property(e => e.Cantidad).HasPrecision(18, 4);
            builder.Entity<MateriaPrima>().Property(e => e.CostoUnitario).HasPrecision(18, 2);
            builder.Entity<Producto>().Property(e => e.PrecioVenta).HasPrecision(18, 2);
            builder.Entity<Venta>().Property(e => e.Total).HasPrecision(18, 2);
            builder.Entity<Venta>().Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            builder.Entity<VentaDetalle>().Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            builder.Entity<Cotizacion>().Property(e => e.TotalEstimado).HasPrecision(18, 2);
            builder.Entity<Cotizacion>().Property(e => e.MontoGanancia).HasPrecision(18, 2);
            builder.Entity<CotizacionDetalle>().Property(e => e.Cantidad).HasPrecision(18, 4);
            builder.Entity<CotizacionDetalle>().Property(e => e.PrecioUnitarioEstimado).HasPrecision(18, 2);
            builder.Entity<MovimientoInventario>().Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            builder.Entity<MovimientoInventario>().Property(e => e.CostoTotal).HasPrecision(18, 2);
            builder.Entity<LoteInventario>().Property(e => e.CostoUnitario).HasPrecision(18, 2);
            builder.Entity<LoteInventario>().Property(e => e.CostoTotal).HasPrecision(18, 2);

            builder.Entity<CostoPromedioMateriaPrima>().Property(e => e.CostoPromedioActual).HasPrecision(18, 2);
            builder.Entity<CostoPromedioMateriaPrima>().Property(e => e.ValorInventarioTotal).HasPrecision(18, 2);

            builder.Entity<CostoPromedioMateriaPrima>()
                .HasOne(cp => cp.MateriaPrima)
                .WithMany()
                .HasForeignKey(cp => cp.MateriaPrimaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CostoPromedioMateriaPrima>()
                .HasIndex(cp => cp.MateriaPrimaId)
                .IsUnique();

            builder.Entity<Compra>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.Compras)
                .HasForeignKey(c => c.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CompraDetalle>()
                .HasOne(cd => cd.Compra)
                .WithMany(c => c.Detalles)
                .HasForeignKey(cd => cd.CompraId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CompraDetalle>()
                .HasOne(cd => cd.MateriaPrima)
                .WithMany(mp => mp.Compras)
                .HasForeignKey(cd => cd.MateriaPrimaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MovimientoInventario>()
                .HasOne(mi => mi.MateriaPrima)
                .WithMany()
                .HasForeignKey(mi => mi.MateriaPrimaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MovimientoInventario>()
                .HasOne(mi => mi.Compra)
                .WithMany()
                .HasForeignKey(mi => mi.CompraId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<LoteInventario>()
                .HasOne(li => li.MateriaPrima)
                .WithMany()
                .HasForeignKey(li => li.MateriaPrimaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LoteInventario>()
                .HasOne(li => li.Proveedor)
                .WithMany()
                .HasForeignKey(li => li.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<LoteInventario>()
                .HasOne(li => li.Compra)
                .WithMany()
                .HasForeignKey(li => li.CompraId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<LoteInventario>()
                .HasIndex(li => li.NumeroLote)
                .IsUnique();

            builder.Entity<MovimientoInventario>()
                .HasIndex(mi => new { mi.MateriaPrimaId, mi.FechaMovimiento });

            builder.Entity<ProductoModular>().Property(e => e.PrecioBaseModulo).HasPrecision(18, 2);
            builder.Entity<ProductoModular>().Property(e => e.PrecioModuloAdicional).HasPrecision(18, 2);
            builder.Entity<ComponenteModulo>().Property(e => e.CantidadBase).HasPrecision(18, 4);
            builder.Entity<ComponenteModulo>().Property(e => e.CantidadPorModuloAdicional).HasPrecision(18, 4);

            builder.Entity<ComponenteModulo>()
                .HasOne(cm => cm.ProductoModular)
                .WithMany(pm => pm.ComponentesBase)
                .HasForeignKey(cm => cm.ProductoModularId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ComponenteModulo>()
                .HasOne(cm => cm.Componente)
                .WithMany()
                .HasForeignKey(cm => cm.ComponenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CotizacionModular>().Property(e => e.SubtotalComponentes).HasPrecision(18, 2);
            builder.Entity<CotizacionModular>().Property(e => e.SubtotalManoObra).HasPrecision(18, 2);
            builder.Entity<CotizacionModular>().Property(e => e.SubtotalMateriales).HasPrecision(18, 2);
            builder.Entity<CotizacionModular>().Property(e => e.TotalEstimado).HasPrecision(18, 2);
            builder.Entity<CotizacionModular>().Property(e => e.PorcentajeGanancia).HasPrecision(18, 2);
            builder.Entity<CotizacionModular>().Property(e => e.MontoGanancia).HasPrecision(18, 2);

            builder.Entity<CotizacionModular>()
                .HasOne(cm => cm.ProductoModular)
                .WithMany(pm => pm.CotizacionesModulares)
                .HasForeignKey(cm => cm.ProductoModularId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CotizacionModularDetalle>().Property(e => e.CantidadBase).HasPrecision(18, 4);
            builder.Entity<CotizacionModularDetalle>().Property(e => e.CantidadAdicional).HasPrecision(18, 4);
            builder.Entity<CotizacionModularDetalle>().Property(e => e.PrecioUnitario).HasPrecision(18, 2);

            builder.Entity<CotizacionModularDetalle>()
                .HasOne(cmd => cmd.CotizacionModular)
                .WithMany(cm => cm.Detalles)
                .HasForeignKey(cmd => cmd.CotizacionModularId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CotizacionModularDetalle>()
                .HasOne(cmd => cmd.Componente)
                .WithMany()
                .HasForeignKey(cmd => cmd.ComponenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ComponenteMateriaPrima>().Property(e => e.CantidadNecesaria).HasPrecision(18, 4);
            builder.Entity<ComponenteMateriaPrima>().Property(e => e.FactorConversion).HasPrecision(18, 4);
            builder.Entity<ComponenteMateriaPrima>().Property(e => e.PorcentajeMerma).HasPrecision(5, 4);

            builder.Entity<ComponenteMateriaPrima>()
                .HasOne(cmp => cmp.Componente)
                .WithMany()
                .HasForeignKey(cmp => cmp.ComponenteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ComponenteMateriaPrima>()
                .HasOne(cmp => cmp.MateriaPrima)
                .WithMany()
                .HasForeignKey(cmp => cmp.MateriaPrimaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ComponenteMateriaPrima>()
                .HasIndex(cmp => new { cmp.ComponenteId, cmp.MateriaPrimaId })
                .IsUnique();

            builder.Entity<ProductoHydroLink>().Property(e => e.Precio).HasPrecision(18, 2);

            builder.Entity<ComponenteRequerido>().Property(e => e.Cantidad).HasPrecision(18, 4);

            builder.Entity<ComponenteRequerido>()
                .HasOne(cr => cr.ProductoHydroLink)
                .WithMany(phl => phl.ComponentesRequeridos)
                .HasForeignKey(cr => cr.ProductoHydroLinkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ComponenteRequerido>()
                .HasOne(cr => cr.Componente)
                .WithMany()
                .HasForeignKey(cr => cr.ComponenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Cotizacion>().Property(e => e.SubtotalComponentes).HasPrecision(18, 2);
            builder.Entity<Cotizacion>().Property(e => e.SubtotalManoObra).HasPrecision(18, 2);
            builder.Entity<Cotizacion>().Property(e => e.SubtotalMateriales).HasPrecision(18, 2);
            builder.Entity<Cotizacion>().Property(e => e.PorcentajeGanancia).HasPrecision(18, 2);

            //builder.Entity<Cotizacion>()
            //    .HasOne(c => c.Cliente)
            //    .WithMany()
            //    .HasForeignKey(c => c.ClienteId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.Entity<Cotizacion>()
            //    .HasOne(c => c.Producto)
            //    .WithMany(phl => phl.Cotizaciones)
            //    .HasForeignKey(c => c.ProductoId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.Entity<Cotizacion>()
            //.HasOne(c => c.Cliente)
            //.WithMany(cl => cl.Cotizaciones)
            //.HasForeignKey(c => c.ClienteId)
            //.OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Cotizacion>()
            .HasOne(c => c.Cliente)
            .WithMany(cl => cl.Cotizaciones)
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);



            builder.Entity<Venta>()
                .HasOne(v => v.Cotizacion)
                .WithOne(c => c.Venta)
                .HasForeignKey<Venta>(v => v.CotizacionId)
                .OnDelete(DeleteBehavior.SetNull); 


            builder.Entity<MovimientoComponente>().Property(e => e.Cantidad).HasPrecision(18, 4);
            builder.Entity<MovimientoComponente>().Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            builder.Entity<MovimientoComponente>().Property(e => e.CostoTotal).HasPrecision(18, 2);

            builder.Entity<MovimientoComponente>()
                .HasOne(mc => mc.Componente)
                .WithMany()
                .HasForeignKey(mc => mc.ComponenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MovimientoComponente>()
                .HasOne(mc => mc.Venta)
                .WithMany()
                .HasForeignKey(mc => mc.VentaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<MovimientoComponente>()
                .HasOne(mc => mc.Compra)
                .WithMany()
                .HasForeignKey(mc => mc.CompraId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Venta>()
                .HasOne(v => v.Producto)
                .WithMany()
                .HasForeignKey(v => v.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.Entity<Cotizacion>()
            //    .HasOne(c => c.Venta)
            //    .WithOne(v => v.Cotizacion)
            //    .HasForeignKey<Cotizacion>(c => c.VentaId)
            //    .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ProductoComprado>()
                .HasOne(pc => pc.Usuario)
                .WithMany()
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductoComprado>()
                .HasOne(pc => pc.Producto)
                .WithMany()
                .HasForeignKey(pc => pc.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductoComprado>()
                .HasOne(pc => pc.Venta)
                .WithMany()
                .HasForeignKey(pc => pc.VentaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ProductoComprado>()
                .HasIndex(pc => new { pc.UserId, pc.ProductoId })
                .IsUnique();

            builder.Entity<Comentario>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comentario>()
                .HasOne(c => c.ProductoHydroLink)
                .WithMany()
                .HasForeignKey(c => c.ProductoHydroLinkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comentario>()
                .HasIndex(c => new { c.ProductoHydroLinkId, c.UsuarioId })
                .IsUnique(); 
        }
    }
}
